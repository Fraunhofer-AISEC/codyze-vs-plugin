using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CodyzeVSPlugin.Highlighting
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(HighlightingFormatHandler.InformationFormat)]
    [UserVisible(true)]
    internal class HighlightWordInformationFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordInformationFormatDefinition()
        {
            this.BackgroundColor = Colors.PeachPuff;
            this.ForegroundColor = Colors.Moccasin;
            this.DisplayName = "Information";
            this.ZOrder = 5;
        }

    }
}
