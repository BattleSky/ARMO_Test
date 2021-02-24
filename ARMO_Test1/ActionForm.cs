using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARMO_Test1
{
    class ActionForm : Form
    {
        public ActionForm()
        {
            var elementWidth = ClientSize.Width;
            
            var choseDirectoryLabel = new Label()
            {
                Location = new Point(0, 0),
                Size = new Size(elementWidth, 30),
                Text = "Directory"
            };
            var directoryTextBox = new TextBox()
            {
                Location = new Point(0, choseDirectoryLabel.Bottom),
                Size = choseDirectoryLabel.Size
            };
            var choseRegexLabel = new Label()
            {
                Location = new Point(0, directoryTextBox.Bottom),
                Size = choseDirectoryLabel.Size,
                Text = "Regex"
            };
            var regexTextBox = new TextBox()
            {
                Location = new Point(0, choseRegexLabel.Bottom),
                Size = choseDirectoryLabel.Size
            };
            var startButton = new Button()
            {
                Location = new Point(0, regexTextBox.Bottom),
                Size = choseDirectoryLabel.Size,
                Text = "Start"
            };

            Controls.Add(choseDirectoryLabel);
            Controls.Add(directoryTextBox);
            Controls.Add(choseRegexLabel);
            Controls.Add(regexTextBox);
            Controls.Add(startButton);
        }
    }
}
