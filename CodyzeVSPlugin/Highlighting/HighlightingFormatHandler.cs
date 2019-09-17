using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodyzeVSPlugin.Highlighting
{
    class HighlightingFormatHandler
    {
        public const string ErrorFormat = "MarkerFormatDefinition/HighlightWordErrorFormatDefinition";
        public const string WarningFormat = "MarkerFormatDefinition/HighlightWordWarningFormatDefinition";
        public const string InformationFormat = "MarkerFormatDefinition/HighlightWordInformationFormatDefinition";
        public const string HintFormat = "MarkerFormatDefinition/HighlightWordHintFormatDefinition";
        public const string DefaultFormat = "MarkerFormatDefinition/HighlightWordDefaultFormatDefinition";
        public static TextMarkerTag ChooseHighlightingFormat(SnapshotSpan span, int severity)
        {
            TextMarkerTag tag = null;
            switch (severity){
                case 1:
                    tag = new TextMarkerTag(ErrorFormat);
                    break;
                case 2:
                    tag = new TextMarkerTag(WarningFormat);
                    break;
                case 3://info
                    tag = new TextMarkerTag(InformationFormat);
                    break;
                case 4://hint
                    tag = new TextMarkerTag(HintFormat);
                    break;
                default:
                    tag = new TextMarkerTag(DefaultFormat);
                    break;
            }
            return tag;
        }
    }
}
