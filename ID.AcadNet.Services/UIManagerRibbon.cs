using System;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AcadNetTools
{
    public partial class AcadNetPlugIn : IExtensionApplication
    {
        // Инициализация нашего плагина
        void IExtensionApplication.Initialize()
        {
            /* ленту грузим с помощью обработчика событий:
             * Этот вариант нужно использовать, если ваш плагин
             * стоит в автозагрузке, т.к. он (плагин) инициализируется
             * до построения ленты
             */
            //Autodesk.Windows.ComponentManager.ItemInitialized += new EventHandler<Autodesk.Windows.RibbonItemEventArgs>(ComponentManager_ItemInitialized);

            // Т.к. мы грузим плагин через NETLOAD, то строим вкладку в ленте сразу
            BuildRibbonTab();
        }
        // Происходит при закрытии автокада
        void IExtensionApplication.Terminate()
        {
            // Тут в принципе ничего не требуется делать
        }
        /* Обработчик события
         * Следит за событиями изменения окна автокада.
         * Используем его для того, чтобы "поймать" момент построения ленты,
         * учитывая, что наш плагин уже инициализировался
         */
        void ComponentManager_ItemInitialized(object sender, Autodesk.Windows.RibbonItemEventArgs e)
        {
            // Проверяем, что лента загружена
            if (Autodesk.Windows.ComponentManager.Ribbon != null)
            {
                // Строим нашу вкладку
                BuildRibbonTab();
                //и раз уж лента запустилась, то отключаем обработчик событий
                Autodesk.Windows.ComponentManager.ItemInitialized -=
                    new EventHandler<Autodesk.Windows.RibbonItemEventArgs>(ComponentManager_ItemInitialized);
            }
        }
        // Построение вкладки
        void BuildRibbonTab()
        {
            // Если лента еще не загружена
            if (!IsLoaded())
            {
                // Строим вкладку
                CreateRibbonTab();
                // Подключаем обработчик событий изменения системных переменных
                acadApp.SystemVariableChanged += new SystemVariableChangedEventHandler(AcadAppSystemVariableChanged);
            }
        }
        // Проверка "загруженности" ленты
        static bool IsLoaded()
        {
            var loaded = false;
            var ribCntrl = ComponentManager.Ribbon;
            // Делаем итерацию по вкладкам ленты
            foreach (RibbonTab tab in ribCntrl.Tabs)
            {
                // И если у вкладки совпадает идентификатор и заголовок, то значит вкладка загружена
                if (tab.Id.Equals("RibbonExample_ID") & tab.Title.Equals("RibbonExample"))
                { loaded = true; break; }
                else loaded = false;
            }
            return loaded;
        }
        /* Удаление своей вкладки с ленты
         * В данном примере не используем
         */
        void RemoveRibbonTab()
        {
            try
            {
                var ribCntrl = ComponentManager.Ribbon;
                // Делаем итерацию по вкладкам ленты
                foreach (RibbonTab tab in ribCntrl.Tabs)
                {
                    if (tab.Id.Equals("RibbonExample_ID") & tab.Title.Equals("RibbonExample"))
                    {
                        // И если у вкладки совпадает идентификатор и заголовок, то удаляем эту вкладку
                        ribCntrl.Tabs.Remove(tab);
                        // Отключаем обработчик событий
                        acadApp.SystemVariableChanged -= AcadAppSystemVariableChanged;
                        // Останавливаем итерацию
                        break;
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
            }
        }
        /* Обработка события изменения системной переменной
         * Будем следить за системной переменной WSCURRENT (текущее рабочее пространство),
         * чтобы наша вкладка не "терялась" при изменение рабочего пространства
         */
        void AcadAppSystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
        {
            if (e.Name.Equals("WSCURRENT")) BuildRibbonTab();
        }
        // Создание нашей вкладки
        void CreateRibbonTab()
        {
            try
            {
                // Получаем доступ к ленте
                var ribCntrl = ComponentManager.Ribbon;
                // добавляем свою вкладку c: Заголовок вкладки Идентификатор вкладки
                var ribTab = new RibbonTab {Title = "RibbonExample", Id = "RibbonExample_ID"};
                
                ribCntrl.Tabs.Add(ribTab); // Добавляем вкладку в ленту
                // добавляем содержимое в свою вкладку (одну панель)
                AddExampleContent(ribTab);
                // Делаем вкладку активной (не желательно, ибо неудобно)
                //ribTab.IsActive = true;
            }
            catch (System.Exception ex)
            {
                acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
            }
        }
        // Строим новую панель в нашей вкладке
        void AddExampleContent(RibbonTab ribTab)
        {
            try
            {
                // создаем panel source
                var ribSourcePanel = new RibbonPanelSource {Title = "RibbonExample"};
                // теперь саму панель
                var ribPanel = new RibbonPanel {Source = ribSourcePanel};
                ribTab.Panels.Add(ribPanel);
                // создаем пустую tooltip (всплывающая подсказка)
                // создаем split button
                RibbonSplitButton risSplitBtn = new RibbonSplitButton();
                /* Для RibbonSplitButton ОБЯЗАТЕЛЬНО надо указать
                 * свойство Text, а иначе при поиске команд в автокаде
                 * будет вылетать ошибка.
                 */
                risSplitBtn.Text = "RibbonSplitButton";
                // Ориентация кнопки
                risSplitBtn.Orientation = System.Windows.Controls.Orientation.Vertical;
                // Размер кнопки
                risSplitBtn.Size = RibbonItemSize.Large;
                // Показывать изображение
                risSplitBtn.ShowImage = true;
                // Показывать текст
                risSplitBtn.ShowText = true;
                // Стиль кнопки
                risSplitBtn.ListButtonStyle = Autodesk.Private.Windows.RibbonListButtonStyle.SplitButton;
                risSplitBtn.ResizeStyle = RibbonItemResizeStyles.NoResize;
                risSplitBtn.ListStyle = RibbonSplitButtonListStyle.List;
                /* Далее создаем две кнопки и добавляем их
                 * не в панель, а в RibbonSplitButton
                 */
                #region Кнопка-пример №1
                // Создаем новый экземпляр подсказки
                var tt = new RibbonToolTip();
                // Отключаем вызов справки (в данном примере её нету)
                tt.IsHelpEnabled = false;
                // Создаем кнопку

                var ribBtn = new RibbonButton
                                 {
                                     CommandParameter = tt.Command = "_Line",   //В свойство CommandParameter (параметры команды) и в свойство Command (отображает команду) подсказки пишем вызываемую команду
                                     Name = "ExampleButton1",                       // Имя кнопки
                                     Text = tt.Title = "Кнопка-пример №1",          // Заголовок кнопки и подсказки
                                     CommandHandler = new RibbonCommandHandler(),   // Создаем новый (собственный) обработчик команд (см.ниже)
                                     Orientation = System.Windows.Controls.Orientation.Horizontal, // Ориентация кнопки
                                     Size = RibbonItemSize.Large, // Размер кнопки
                                     LargeImage = LoadImage("icon_32"),  // Т.к. используем размер кнопки Large, то добавляем большое изображение с помощью специальной функции (см.ниже)
                                     ShowImage = true, //Показывать  картинку
                                     ShowText = true // Показывать текст
                                 };
                
                // Заполняем содержимое всплывающей подсказки
                tt.Content = "Я кнопочка №1. Нажми меня и я нарисую отрезок";
                // Подключаем подсказку к кнопке
                ribBtn.ToolTip = tt;
                // Добавляем кнопку в RibbonSplitButton
                risSplitBtn.Items.Add(ribBtn);
                #endregion
                // Делаем текущей первую кнопку
                risSplitBtn.Current = ribBtn;
                // Далее создаем вторую кнопку по аналогии с первой
                #region Кнопка-пример №2

                tt = new RibbonToolTip {IsHelpEnabled = false};
                ribBtn = new RibbonButton
                             {
                                 CommandParameter = tt.Command = "_Pline",
                                 Name = "ExampleButton2",
                                 Text = tt.Title = "Кнопка-пример №2",
                                 CommandHandler = new RibbonCommandHandler(),
                                 Orientation = System.Windows.Controls.Orientation.Horizontal,
                                 Size = RibbonItemSize.Large,
                                 LargeImage = LoadImage("icon_32"),
                                 ShowImage = true,
                                 ShowText = true
                             };
                tt.Content = "Я кнопочка №2. Нажми меня и я нарисую полилинию";
                ribBtn.ToolTip = tt;
                risSplitBtn.Items.Add(ribBtn);
                #endregion
                // Добавляем RibbonSplitButton в нашу панель
                ribSourcePanel.Items.Add(risSplitBtn);
                // Создаем новую строку
                var ribRowPanel = new RibbonRowPanel();
                // Создаем третью кнопку по аналогии с предыдущими.
                // Отличие только в размере кнопки (и картинки)
                #region Кнопка-пример №3

                tt = new RibbonToolTip {IsHelpEnabled = false};
                ribBtn = new RibbonButton
                             {
                                 CommandParameter = tt.Command = "_Circle",
                                 Name = "ExampleButton3",
                                 Text = tt.Title = "Кнопка-пример №3",
                                 CommandHandler = new RibbonCommandHandler(),
                                 Orientation = System.Windows.Controls.Orientation.Vertical,
                                 Size = RibbonItemSize.Standard,
                                 Image = LoadImage("icon_16"),
                                 ShowImage = true,
                                 ShowText = false
                             };
                tt.Content = "Я кнопочка №3. Нажми меня и я нарисую кружочек";
                ribBtn.ToolTip = tt;
                ribRowPanel.Items.Add(ribBtn);
                #endregion
                // Добавляем строку в нашу панель
                ribSourcePanel.Items.Add(ribRowPanel);
            }
            catch (System.Exception ex)
            {
                acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
            }
        }
        // Получение картинки из ресурсов
        // Данная функция найдена на просторах интернет
        static System.Windows.Media.Imaging.BitmapImage LoadImage(string imageName)
        {
            return new System.Windows.Media.Imaging.BitmapImage(
                new Uri("pack://application:,,,/ACadRibbon;component/" + imageName + ".png"));
        }
        /* Собственный обраотчик команд
         * Это один из вариантов вызова команды по нажатию кнопки
         */
        class RibbonCommandHandler : System.Windows.Input.ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
            
            public void Execute(object parameter)
            {
                var doc = acadApp.DocumentManager.MdiActiveDocument;
                if (parameter is RibbonButton)
                {
                    // Просто берем команду, записанную в CommandParameter кнопки
                    // и выпоняем её используя функцию SendStringToExecute
                    var button = parameter as RibbonButton;
                    acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute(
                        button.CommandParameter + " ", true, false, true);
                }
            }
        }
    }
}
