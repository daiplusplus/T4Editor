﻿using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace T4Editor
{
    internal class T4Classifier : IClassifier
    {
        private readonly ITextBuffer _buffer;
        IClassificationTypeRegistryService _classificationTypeRegistry;

        internal T4Classifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            this._buffer = buffer;
            this._classificationTypeRegistry = registry;
        }

#pragma warning disable 67

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        protected virtual void OnClassificationChanged(ClassificationChangedEventArgs e)
        {
            ClassificationChanged?.Invoke(this, e);
        }

        public ClassificationSpan CreateClassificationSpan(ITextSnapshot snapshot, int index, int length, IClassificationType type)
        {
            return new ClassificationSpan(new SnapshotSpan(new SnapshotPoint(snapshot, index), length), type);
        }

#pragma warning restore 67

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            if (snapshot.Length == 0)
                return spans;

            var directiveRegex = "(?<=<#@)((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s|\\w|\\n?)(?=#>)";
            var classFeatureBlockRegex = "(?<=<#\\+)((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s|\\w|\\n?)(?=#>)";
            var statementBlockRegex = "<#((?!=|@|\\+))((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s?|\\w?|\n?)#>";
            var injectedRegex = "<#=((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(\\s?)#>";
            var outputRegex = "(?<=#>)(((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s|\\w|\n?))(?=<#)";

            var document = snapshot.GetText();

            MatchCollection directiveMatches = Regex.Matches(document, directiveRegex);
            MatchCollection classFeatureBlockMatches = Regex.Matches(document, classFeatureBlockRegex);
            MatchCollection statementBlockMatches = Regex.Matches(document, statementBlockRegex);
            MatchCollection injectedMatches = Regex.Matches(document, injectedRegex);
            MatchCollection outputMatches = Regex.Matches(document, outputRegex);

            IClassificationType type = null;

            foreach (Match match in directiveMatches)
            {
                if (match.Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType("T4.Directive");
                    spans.Add(CreateClassificationSpan(snapshot, match.Index, match.Value.Length, type));
                }
            }

            foreach (Match match in classFeatureBlockMatches)
            {
                if (match.Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType("T4.ClassFeatureBlock");
                    spans.Add(CreateClassificationSpan(snapshot, match.Index, match.Value.Length, type));
                }
            }

            foreach (Match match in statementBlockMatches)
            {
                if (match.Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType("T4.StatementBlock");
                    spans.Add(CreateClassificationSpan(snapshot, match.Index, match.Value.Length, type));
                }
            }

            foreach (Match match in injectedMatches)
            {
                if (match.Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType("T4.Injected");
                    spans.Add(CreateClassificationSpan(snapshot, match.Index, match.Value.Length, type));
                }
            }

            foreach (Match match in outputMatches)
            {
                if (match.Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType("T4.Output");
                    spans.Add(CreateClassificationSpan(snapshot, match.Index, match.Value.Length, type));
                }
            }

            return spans;
        }
    }
}

