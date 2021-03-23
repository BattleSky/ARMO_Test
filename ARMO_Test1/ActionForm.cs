using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARMO_Test1
{
    public sealed class ActionForm : Form
    {
        private static bool _isPaused;
        private int _secondsTicked;
        private readonly ReadWriteSaveInputs _saveInputs = new ReadWriteSaveInputs();
        public static ManualResetEvent PauseEvent = new ManualResetEvent(false);
        public static CancellationTokenSource CancellationToken = new CancellationTokenSource();

        public ActionForm()
            {
            Size = new Size(600, 850);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            var baseSize = new Size(ClientSize.Width, 30);
            DoubleBuffered = true;
            Text = "Поиск файлов (ARMO_Test)";
            
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, args) =>
            {
                if (!_isPaused)
                    _secondsTicked++;
            };

            #region Инициализация элементов формы

            var choseDirectoryLabel = new Label()
            {
                Location = new Point(0, 0),
                Size = baseSize,
                Text = "Укажите папку"
            };
            var directoryTextBox = new TextBox()
            {
                Location = new Point(0, choseDirectoryLabel.Bottom),
                Size = baseSize,
                Text = _saveInputs.DirectoryPath
            };
            var choseRegexLabel = new Label()
            {
                Location = new Point(0, directoryTextBox.Bottom),
                Size = baseSize,
                Text = "Укажите REGEX-выражение"
            };
            var regexTextBox = new TextBox()
            {
                Location = new Point(0, choseRegexLabel.Bottom),
                Size = baseSize,
                Text = _saveInputs.Regex
            };
            var startButton = new Button()
            {
                Location = new Point(0, regexTextBox.Bottom),
                Size = new Size(baseSize.Width / 3, baseSize.Height),
                Text = "Поиск",
                Enabled = true
            };
            var pauseButton = new Button()
            {
                Location = new Point(startButton.Right, regexTextBox.Bottom),
                Size = new Size(baseSize.Width / 3, baseSize.Height),
                Text = "Приостановить",
                Enabled = false
            };
            var stopButton = new Button()
            {
                Location = new Point(pauseButton.Right, regexTextBox.Bottom),
                Size = new Size(baseSize.Width / 3, baseSize.Height),
                Text = "Остановить",
                Enabled = false
            };
            var treeView = new TreeView()
            {
                Location = new Point(0, startButton.Bottom),
                Size = new Size(baseSize.Width, 500)
            };
            var timerLabel = new Label()
            {
                Location = new Point(0, treeView.Bottom),
                Size = baseSize,
                Text = "Время поиска: ----"
            };
            var filesFoundLabel = new Label()
            {
                Location = new Point(0, timerLabel.Bottom),
                Size = baseSize,
                Text = "Найдено файлов: -"
        };
            var currentDirAndAllFilesLabel = new Label()
            {
                Location = new Point(0, filesFoundLabel.Bottom),
                Size = new Size(baseSize.Width, 100),
                Text = "Просканировано файлов: -\n" +
                       "Текущая директория сканирования:---\n"
        };
            Controls.Add(choseDirectoryLabel);
            Controls.Add(directoryTextBox);
            Controls.Add(choseRegexLabel);
            Controls.Add(regexTextBox);
            Controls.Add(startButton);
            Controls.Add(pauseButton);
            Controls.Add(stopButton);
            Controls.Add(treeView);
            Controls.Add(timerLabel);
            Controls.Add(currentDirAndAllFilesLabel);
            Controls.Add(filesFoundLabel);
            #endregion



            #region События клика по кнопкам
            startButton.Click += async (sender, args) =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    CancellationToken = new CancellationTokenSource();
                    SearchFiles.SearchCancelledOrDone = false;
                }
                timer.Start();
                GuiToSearch();
                PauseEvent.Set();
                _saveInputs.Update(directoryTextBox.Text, regexTextBox.Text);
                await Task.WhenAny(
                    Task.WhenAll(SearchFiles.SearchByRegex(regexTextBox.Text, directoryTextBox.Text), GuiRefreshTask()),
                    Task.Delay(Timeout.Infinite, CancellationToken.Token));
            };

            pauseButton.Click += (sender, args) =>
            {
                if (!_isPaused)
                {
                    PauseEvent.Reset();
                    _isPaused = true;
                    pauseButton.Text = "Продолжить";
                }
                else
                {
                    PauseEvent.Set();
                    _isPaused = false;
                    pauseButton.Text = "Приостановить";
                }
            };

            stopButton.Click += (sender, args) =>
            {
                timer.Stop();
                CancellationToken.Cancel();
                GuiToDefault();
            };

            #endregion

            Task GuiRefreshTask()
            {
                return Task.Run(GuiRefresh);
            }


            void GuiRefresh()
            {
                var allFiles = SearchFiles.AllFiles;
                var ticks = _secondsTicked;
                while (!CancellationToken.IsCancellationRequested)
                {
                    PauseEvent.WaitOne();
                    if (SearchFiles.SearchCancelledOrDone &&
                        (SearchFiles.FilesQueue == null || SearchFiles.FilesQueue.Count == 0))
                    {
                        Invoke(new Action(ChangeFilesFoundLabel));
                        CancellationToken.Cancel();
                        Invoke(new Action(GuiToDefault));
                    }
                    else
                        Invoke(new Action(UpdateTreeAndFilesFoundLabel));
                    
                    if (allFiles != SearchFiles.AllFiles)
                    {
                        Invoke(new Action(ChangeCurrentDirAndAllFilesLabel));
                        allFiles = SearchFiles.AllFiles;
                    }
                    if (ticks != _secondsTicked)
                    {
                        Invoke(new Action(TimerTick));
                        ticks = _secondsTicked;
                        Invoke(new Action(ChangeFilesFoundLabel));
                    }
                }
            }

            void UpdateTreeAndFilesFoundLabel()
            {
                NodeOperations.AddNodeFromPath(treeView);
            }


            void GuiToDefault()
            {
                pauseButton.Text = "Приостановить";
                stopButton.Enabled = pauseButton.Enabled = false;
                startButton.Enabled = true;
                timer.Stop();
            }

            void GuiToSearch()
            {
                _secondsTicked = 0;
                treeView.Nodes.Clear();
                pauseButton.Enabled = stopButton.Enabled = true;
                startButton.Enabled = false;
                currentDirAndAllFilesLabel.Text = "Просканировано файлов: -\n" +
                                        "Текущая директория сканирования:---\n";
                filesFoundLabel.Text = "Найдено файлов: -";
                timerLabel.Text = "Время поиска: 00:00";
            }

            void ChangeCurrentDirAndAllFilesLabel()
            {
                currentDirAndAllFilesLabel.Text =
                    $"Просканировано файлов: {SearchFiles.AllFiles}\n" +
                    $"Текущая директория сканирования:\n {SearchFiles.CurrentFolder}";
            }

            void ChangeFilesFoundLabel()
            {
                filesFoundLabel.Text = $"Найдено файлов: {SearchFiles.FilesFound}";
            }

            void TimerTick()
            {
                var time = TimeSpan.FromSeconds(_secondsTicked);
                timerLabel.Text = "Время поиска: " + $"{time.Minutes:D1}:{time.Seconds:D2}";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs eventArgs)
        {
            _saveInputs.Serialize();
            CancellationToken.Cancel();
            eventArgs.Cancel = false;
        }
    }
}

