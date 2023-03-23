using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace FarManager
{
    public partial class MainWindow : Window
    {
        private string backPath = null!;
        private string copyPath = null!;
        private string movePath = null!;
        public ICommand OpenCommand { get; set; }
        public ICommand CopyCommand { get; set; }
        public ICommand MoveCommand { get; set; }
        public ICommand PasteCopyCommand { get; set; }
        public ICommand PasteMoveCommand { get; set; }
        public ICommand ButtonCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        private void DeleteDirectory(DirectoryInfo directory)
        {
            foreach (var f in directory.GetFiles())
                f.Delete();

            foreach (var d in directory.GetDirectories())
                DeleteDirectory(d);

            directory.Delete();
        }


        private void ManageUpTreeView(DirectoryInfo directory, TreeView view)
        {
            if (view == null || directory == null) return;

            try
            {
                var directories = directory.GetDirectories();
                var files = directory.GetFiles();

                view.Items.Clear();


                foreach (var d in directories)
                    view.Items.Add(d);

                foreach (var f in files)
                    view.Items.Add(f);
            }
            catch (Exception)
            {
                MessageBox.Show("Access Denied");
                return;
            }
        }

        private void OpenWithDefaultProgram(string path)
        {
            using Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        private void FileMover(string fileName, string sourcePath, string targetPath)
        {
            var sourceFile = Path.Combine(sourcePath, fileName);
            var destinyFile = Path.Combine(targetPath, fileName);

            File.Move(sourceFile, destinyFile);
        }

        private void DirectoryMover(string sourcePath, string targetPath)
        {
            var directory = new DirectoryInfo(sourcePath);

            var resultPath = Path.Combine(targetPath, directory.Name);

            Directory.Move(directory.FullName, resultPath);
        }

        private void FileCopier(string fileName, string sourcePath, string targetPath)
        {

            string sourceFile = Path.Combine(sourcePath, fileName);
            string destFile = Path.Combine(targetPath, fileName);

            File.Copy(sourceFile, destFile, true);
        }

        private void DirectoryCopier(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
                return;

            var directory = new DirectoryInfo(sourcePath);
            var resultPath = Path.Combine(targetPath, directory.Name);

            Directory.CreateDirectory(resultPath);

            foreach (var f in directory.GetFiles())
            {
                FileCopier(f.Name, sourcePath, resultPath);
            }

            foreach (var d in directory.GetDirectories())
            {
                DirectoryCopier(d.FullName, resultPath);
            }
        }


        private bool CanExecutePasteMoveCommand(object? obj)
        {
            if (obj is TreeView t)
            {
                if (t.SelectedItem is DirectoryInfo && movePath != null)
                    return true;
            }
            return false;
        }        

        private void ExecutePasteMoveCommand(object? obj)
        {
            if (obj is TreeView t)
            {
                if (t.SelectedItem is FileInfo)
                    return;

                if (t.SelectedItem is DirectoryInfo d)
                {
                    if (File.Exists(movePath))
                    {
                        var file = new FileInfo(movePath);

                        if (file is null)
                            return;

                        try
                        {
                            FileMover(file.Name, file.Directory.FullName, d.FullName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else if (Directory.Exists(movePath))
                    {
                        var directory = new DirectoryInfo(movePath);

                        DirectoryMover(directory.FullName, d.FullName);
                    }

                    LeftSideTree.Items.Remove(t.SelectedItem);
                    RigtSideTree.Items.Remove(t.SelectedItem);
                }
            }
            movePath = null!;
        }

        private void ExecuteMoveCommand(object? obj)
        {
            if (obj is TreeView t)
            {
                if (t.SelectedItem is DirectoryInfo d)
                    movePath = d.FullName;
                else if (t.SelectedItem is FileInfo f)
                    movePath = f.FullName;

            }
        }

        private bool CanPasteCopyEcexuteCommand(object? obj)
        {
            if (obj is TreeView t)
            {
                if (t.SelectedItem is DirectoryInfo && copyPath != null)
                    return true;
            }
            return false;
        }

        private void ExecutePasteCopyCommand(object? obj)
        {
            if (obj is TreeView t)
            {
                if (t.SelectedItem is FileInfo)
                    return;

                if (t.SelectedItem is DirectoryInfo d)
                {
                    if (File.Exists(copyPath))
                    {
                        var file = new FileInfo(copyPath);

                        if (file is null)
                            return;

                        try
                        {
                            FileCopier(file.Name, file.Directory.FullName, d.FullName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else if (Directory.Exists(copyPath))
                    {
                        var directory = new DirectoryInfo(copyPath);

                        DirectoryCopier(directory.FullName, d.FullName);
                    }

                }
            }

            copyPath = null!;
        }

        private void ExecuteCopyCommand(object? obj)
        {
            if (obj is TreeView t)
            {
                if (t.SelectedItem is DirectoryInfo d)
                    copyPath = d.FullName;
                else if (t.SelectedItem is FileInfo f)
                    copyPath = f.FullName;

            }
        }

        private void ExecuteDeleteCommand(object? obj)
        {
            if (obj is TreeView t)
            {
                var item = t.SelectedItem;
                try
                {
                    if (item is DirectoryInfo directory)
                        DeleteDirectory(directory);

                    else if (item is FileInfo file)
                        file.Delete();


                    LeftSideTree.Items.Remove(item);
                    RigtSideTree.Items.Remove(item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        private bool CanExecuteButtonCommand(object? obj) => backPath != null && !string.IsNullOrWhiteSpace(backPath);

        private void ExecuteButtonCommand(object? obj)
        {

            if (obj is TreeView t)
            {
                var directory = new DirectoryInfo(backPath);
                backPath = directory.Parent?.FullName;
                ManageUpTreeView(directory, t);
            }
        }

        private void ExecuteOpenCommand(object? parameter)
        {
            if (parameter is TreeView t)
            {
                if (t.SelectedItem is DirectoryInfo directory)
                {
                    backPath = directory.Parent.FullName;
                    ManageUpTreeView(directory, t);
                }
                else if (t.SelectedItem is FileInfo file)
                {
                    backPath = file.Directory.FullName;
                    OpenWithDefaultProgram(file.FullName);
                }

            }
        }

        private bool CanEcexuteCommand(object? parameter)
        {
            if (parameter is TreeView t)
                return t.SelectedItem != null;

            return false;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            ArgumentNullException.ThrowIfNull(path);
            DirectoryInfo directory = new(path);

            backPath = directory?.Parent.FullName;
            foreach (var d in directory.GetDirectories())
            {
                RigtSideTree.Items.Add(d);
                LeftSideTree.Items.Add(d);
            }

            foreach (var f in directory.GetFiles())
            {
                RigtSideTree.Items.Add(f);
                LeftSideTree.Items.Add(f);
            }


            DeleteCommand = new RelayCommand(ExecuteDeleteCommand, CanEcexuteCommand);
            CopyCommand = new RelayCommand(ExecuteCopyCommand, CanEcexuteCommand);
            PasteCopyCommand = new RelayCommand(ExecutePasteCopyCommand, CanPasteCopyEcexuteCommand);
            PasteMoveCommand = new RelayCommand(ExecutePasteMoveCommand, CanExecutePasteMoveCommand);
            OpenCommand = new RelayCommand(ExecuteOpenCommand, CanEcexuteCommand);
            ButtonCommand = new RelayCommand(ExecuteButtonCommand, CanExecuteButtonCommand);
            MoveCommand = new RelayCommand(ExecuteMoveCommand, CanEcexuteCommand);
        }
    }
}
