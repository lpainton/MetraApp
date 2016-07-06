using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using Metra.Axxess;

namespace MetraWPFBrowserApp
{
    class BoardInfoLogic
    {
        public TextBlock BoardInfoBlock { get; set; }
        delegate void NoInfoDelegate();
        delegate void SetInfoDelegate(string pid, string ver);

        public BoardInfoLogic(TextBlock boardInfo)
        {
            this.BoardInfoBlock = boardInfo;
        }

        public void Intialize()
        {
            this.NoInfo();
        }

        public void SetInfo(string pid, string version)
        {
            if (BoardInfoBlock.Dispatcher.CheckAccess())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(pid);
                sb.AppendLine(version);
                BoardInfoBlock.Text = sb.ToString();
            }
            else
            {
                this.BoardInfoBlock.Dispatcher.BeginInvoke(
                    new SetInfoDelegate(SetInfo),
                    DispatcherPriority.Normal,
                    new object[] { pid, version });
            }
        }

        public void NoInfo()
        {
            if (BoardInfoBlock.Dispatcher.CheckAccess())
            {
                this.BoardInfoBlock.Text = "No board attached";
            }
            else
            {
                this.BoardInfoBlock.Dispatcher.BeginInvoke(
                    new NoInfoDelegate(NoInfo),
                    DispatcherPriority.Normal,
                    new object[] {});
            }
        }

        public void Update(IAxxessBoard board)
        {
            if (board == null)
                this.NoInfo();
            else
                this.SetInfo(board.ProductID, board.AppFirmwareVersion);
        }
    }
}
