using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
namespace TpSrsComparer.Domain
{
    public class CompareItem : BindableBase
    {
        public string Name
        {
            get;
            set;
        }

        public ObservableCollection<CompareLocation> LeftLocations
        {
            get;
        } = new ObservableCollection<CompareLocation>();


        public ObservableCollection<CompareLocation> RightLocations
        {
            get;
        } = new ObservableCollection<CompareLocation>();


        public ComparedType ComparedType
        {
            get
            {
                if (LeftLocations.Count > 0 && RightLocations.Count > 0)
                {
                    return ComparedType.All;
                }
                if (LeftLocations.Count > 0 && RightLocations.Count <= 0)
                {
                    return ComparedType.OnlyLeft;
                }
                OnPropertyChanged();
                return ComparedType.OnlyRight;
            }
        }

 

        public CompareItem() 
        {
        }
        public void Update()
        {
             this.OnPropertyChanged("ComparedType");
        }
    }
}
