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
    [Name(HighlightingFormatHandler.ErrorFormat)]
    [UserVisible(true)]
    internal class HighlightWordErrorFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordErrorFormatDefinition()
        {
            this.BackgroundColor = Colors.PaleVioletRed;
            this.ForegroundColor = Colors.HotPink;
            this.DisplayName = "Error";
            this.ZOrder = 5;
        }
        
    }
}
