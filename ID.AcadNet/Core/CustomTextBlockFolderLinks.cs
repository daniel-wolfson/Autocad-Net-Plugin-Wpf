using FileExplorer.Model;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Core
{
    public class CustomTextBlockFolderLinks : TextBlock, IDisposable
    {
        Lazy<ICommand> _lazyRefreshFolderDetailsCommand;
        private ICommand LazyCommand { get { return _lazyRefreshFolderDetailsCommand.Value; } }

        public static readonly DependencyProperty LinkStyleSelectedProperty =
            DependencyProperty.Register("LinkStyleSelected", typeof(Style), typeof(CustomTextBlockFolderLinks));
        public static readonly DependencyProperty LinkStyleProperty =
            DependencyProperty.Register("LinkStyle", typeof(Style), typeof(CustomTextBlockFolderLinks));
        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.Register("LinkedFolder", typeof(IFolder), typeof(CustomTextBlockFolderLinks), new UIPropertyMetadata());

        public Style LinkStyle
        {
            get { return (Style)GetValue(LinkStyleProperty); }
            set { SetValue(LinkStyleProperty, value); }
        }
        public Style LinkStyleSelected
        {
            get { return (Style)GetValue(LinkStyleSelectedProperty); }
            set { SetValue(LinkStyleSelectedProperty, value); }
        }
        public IFolder LinkedFolder
        {
            get { return (IFolder)GetValue(FolderProperty); }
            set { SetValue(FolderProperty, value); }
        }

        static CustomTextBlockFolderLinks()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomTextBlockFolderLinks), new FrameworkPropertyMetadata(typeof(CustomTextBlockFolderLinks)));
        }

        public CustomTextBlockFolderLinks()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            //DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomTextBlockFolderLinks), new FrameworkPropertyMetadata(typeof(CustomTextBlockFolderLinks)));
            TargetUpdated += OnTargetUpdated;

            _lazyRefreshFolderDetailsCommand = new Lazy<ICommand>(() =>
            {

                Grid grid = this.FindParent<Grid>();
                ProjectExplorerPanelContext dataContext = grid.DataContext as ProjectExplorerPanelContext;
                return (dataContext != null) ? dataContext.FileExplorerViewModel.RefreshFolderDetailsCommand : null;
            });
        }

        private void OnTargetUpdated(object sender, DataTransferEventArgs dataTransferEventArgs)
        {
            if (DesignerProperties.GetIsInDesignMode(this) || LinkedFolder == null) return;

            List<IFolder> folders = new List<IFolder>();

            this.Inlines.ToList().ForEach(x => x.MouseDown -= RunOnMouseDown);
            this.Inlines.Clear();

            if (LinkedFolder != null)
            {
                //this.Inlines.Add(new Run(System.IO.Path.GetPathRoot(LinkedFolder.FullPath)));
                var parent = LinkedFolder;
                while (parent != null && parent != parent.Parent)
                {
                    folders.Add(parent);
                    parent = parent.Parent;
                }
                folders.Reverse();

                var drives = folders.OfType<LocalDriver>();
                if (!drives.Any())
                {
                    var drive = Path.GetPathRoot(LinkedFolder.FullPath);
                    if (drive != null)
                        this.Inlines.Add(new Run(drive.Replace("\\", "")));
                }
            }

            foreach (var folder in folders)
            {
                RunFolder run;
                if (!(folder is LocalDriver))
                {
                    this.Inlines.Add(new Run("\\"));
                    run = new RunFolder(folder, LinkStyle);
                    run.MouseDown += RunOnMouseDown;

                    MouseBinding mouseBinding = new MouseBinding()
                    {
                        CommandParameter = run.LinkedFolder,
                        Command = new DelegateCommand<IFolder>(o => { o.IsSelected = true; }),
                        MouseAction = MouseAction.LeftDoubleClick
                    };
                    run.InputBindings.Add(mouseBinding);
                }
                else
                    run = new RunFolder(folder.Name.Replace("\\", ""));

                this.Inlines.Add(run);
                //LinkedTb.LinkPressed += (sender, args) => Process.Start(args.Link);
            }
            var last = this.Inlines.LastOrDefault();
            if (last != null) last.Style = LinkStyleSelected;
        }

        private void RunOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var run = (RunFolder)sender;

            Inlines.ToList().Where(x => x.GetType() != typeof(LocalDriver)).ForEach(x => x.Style = LinkStyle);

            var r = this.Inlines.FirstOrDefault(x => x.Name == run.Name);
            if (r != null)
            {
                r.Style = LinkStyleSelected;
                r.UpdateDefaultStyle();
            }

            //var btn = run.FindParentElement<Button>("EditButton");
            //if (btn != null)
            //    btn.ToolTip = string.Format("Edit {0} of windows explorer ", run.LinkedFolder.FullPath);

            //LinkedFolder = run.LinkedFolder;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                Process.Start(run.LinkedFolder.FullPath);
            else if (run.LinkedFolder != null)
                LazyCommand.Execute(run.LinkedFolder);

            //var l = run.Text;
            //OnLinkPressed(new LinkPressedEventArgs(l));
        }

        public event LinkPressedEvent LinkPressed;

        public void OnLinkPressed(LinkPressedEventArgs args)
        {
            var handler = LinkPressed;
            if (handler != null) handler(this, args);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.Inlines.ToList().ForEach(x => x.MouseDown -= RunOnMouseDown);
                    this.Inlines.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CustomTextBlockFolderLinks() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}