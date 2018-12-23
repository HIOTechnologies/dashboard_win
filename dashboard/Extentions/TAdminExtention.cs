using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIO.Extentions
{
    public class TAdminExtention
    {

        public IEnumerable<IExtention> AllExtentions
        {

            get
            {

                yield return Extention02;
                yield return Extention06a;
                yield return Extention07;
                yield return Extention08;
                yield return Extention09;
                yield return Extention10;
                yield return Extention11;
                yield return Extention13;
                yield return Extentiongp;
                yield return ExtentionManage;
                yield return ExtentionMenu;
                yield return PinInputExtension;
            }
        }

        public IEnumerable<Type> AllExtentionsViewTypes
        {
            get
            {
                yield return typeof(TExtention02View);
                yield return typeof(TExtention06aView);
                yield return typeof(TExtention07View);
                yield return typeof(TExtention08View);
                yield return typeof(TExtention09View);
                yield return typeof(TExtention10View);
                yield return typeof(TExtention11View);
                yield return typeof(TExtention13View);
                yield return typeof(TExtentionGenPassView);
                yield return typeof(TExtentionManageView);
                yield return typeof(TExtentionMenuView);
                yield return typeof(TPinInputExtensionView);
            }
        }

        public TExtention02 Extention02 { get; private set; } = new TExtention02();
        public TExtention06a Extention06a { get; private set; } = new TExtention06a();
        public TExtention07 Extention07 { get; private set; } = new TExtention07();
        public TExtention08 Extention08 { get; private set; } = new TExtention08();
        public TExtention09 Extention09 { get; private set; } = new TExtention09();
        public TExtention10 Extention10 { get; private set; } = new TExtention10();
        public TExtention11 Extention11 { get; private set; } = new TExtention11();
        public TExtention13 Extention13 { get; private set; } = new TExtention13();
        public TExtentionGenPass Extentiongp { get; private set; } = new TExtentionGenPass();
        public TExtentionManage ExtentionManage { get; private set; } = new TExtentionManage();
        public TExtentionMenu ExtentionMenu { get; private set; } = new TExtentionMenu();
        public TPinInputExtension PinInputExtension { get; private set; } = new TPinInputExtension();

        public void CloseAll()
        {
            foreach (var item in AllExtentions)
            {
                item?.Form?.Close();
            }
        }
        public void ShowOnly(IExtention extention)
        {
            foreach (var item in AllExtentions.Except(new IExtention[] { extention }).ToArray())
            {
                item?.Form?.Close();

            }

        }
    }
}
