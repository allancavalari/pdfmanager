using System;
using System.Drawing;
using System.Windows.Forms;

namespace PDFman
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PDFManager());
        }
    }

    public class ToolStripSpringTextBox : ToolStripTextBox
    {
        public override Size GetPreferredSize(Size constrainingSize)
        {

            if (IsOnOverflow || Owner.Orientation == Orientation.Vertical)
            {
                return DefaultSize;
            }

            Int32 width = Owner.DisplayRectangle.Width;

            if (Owner.OverflowButton.Visible)
            {
                width = width - Owner.OverflowButton.Width -
                Owner.OverflowButton.Margin.Horizontal;
            }

            Int32 springBoxCount = 0;

            foreach (ToolStripItem item in Owner.Items)
            {
                if (item.IsOnOverflow) continue;

                if (item is ToolStripSpringTextBox)
                {
                    springBoxCount++;
                    width -= item.Margin.Horizontal;
                }
                else
                {
                    width = width - item.Width - item.Margin.Horizontal;
                }
            }

            if (springBoxCount > 1) width /= springBoxCount;

            if (width < DefaultSize.Width) width = DefaultSize.Width;

            Size size = base.GetPreferredSize(constrainingSize);
            size.Width = width;
            size.Height = 28;
            return size;
        }
    }

}
