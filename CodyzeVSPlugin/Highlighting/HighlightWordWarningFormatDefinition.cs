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
    [Name(HighlightingFormatHandler.WarningFormat)]
    [UserVisible(true)]
    internal class HighlightWordWarningFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordWarningFormatDefinition()
        {
            this.BackgroundColor = Colors.LightSalmon;
            this.ForegroundColor = Colors.Chocolate;
            this.DisplayName = "Warning";
            this.ZOrder = 5;
        }

    }
}
