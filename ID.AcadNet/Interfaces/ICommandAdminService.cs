using Intellidesk.AcadNet.ViewModels;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ICommandAdminService
    {
        MainViewModel MainViewModel { get; set; }

        MapViewModel MapViewModel { get; set; }

        /// <summary> Application Session Command with localized name </summary>
        void RibbonBar();

        /// <summary> Application Session Command with localized name </summary>
        void MainPalette();

        /// <summary> Application Session Command with localized name </summary>
        void MapPalette();

        /// <summary> Deciding of problem: Autocad main window not get focus after palette activity </summary>
        void Regen();

        /// <summary> Deciding of problem: Autocad main window not get focus after palette activity. </summary>
        void Purge();

        void Start();

        /// <summary> Alternative to Compose, Loading the DataContext to MainViewModel from others sources of data </summary>
        MainViewModel ComposeViewModel();
    }
}