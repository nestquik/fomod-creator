using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using AspectInjector.Broker;
using FomodInfrastructure.Aspect;

namespace FomodModel.Base.ModuleCofiguration
{
    /// <summary>
    ///     A list of files and folders.
    /// </summary>
    [Aspect(typeof(AspectINotifyPropertyChanged)), Serializable]
    public class FileList
    {
        #region Properties

        [XmlElement("file", typeof(FileSystemItem)), XmlElement("folder", typeof(FolderSystemItem))]
        public ObservableCollection<SystemItem> Items { get; set; }

        #endregion

        public static FileList Create()
        {
            return new FileList { Items = new ObservableCollection<SystemItem>() };
        }
    }
}