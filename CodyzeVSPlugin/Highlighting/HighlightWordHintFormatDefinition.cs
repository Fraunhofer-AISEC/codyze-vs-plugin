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
    [Name(HighlightingFormatHandler.HintFormat)]
    [UserVisible(true)]
    internal class HighlightWordHintFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordHintFormatDefinition()
        {
            this.BackgroundColor = Colors.MediumAquamarine;
            this.ForegroundColor = Colors.LightSeaGreen;
            this.DisplayName = "Hint";
            this.ZOrder = 5;
        }

    }
}
