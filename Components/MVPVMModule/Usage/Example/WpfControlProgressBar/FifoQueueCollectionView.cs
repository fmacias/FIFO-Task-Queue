using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfControlProgressBar
{
    public class FifoQueueCollectionView : ICollectionView
    {
        public bool CanFilter => false;

        public bool CanGroup => false;

        public bool CanSort => false;

        public CultureInfo Culture { get => CultureInfo.CurrentCulture; set => Culture = value; }

        public object CurrentItem => throw new NotImplementedException();

        public int CurrentPosition => throw new NotImplementedException();

        public Predicate<object> Filter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ObservableCollection<GroupDescription> GroupDescriptions => throw new NotImplementedException();

        public ReadOnlyObservableCollection<object> Groups => throw new NotImplementedException();

        public bool IsCurrentAfterLast => throw new NotImplementedException();

        public bool IsCurrentBeforeFirst => throw new NotImplementedException();

        public bool IsEmpty => throw new NotImplementedException();

        public SortDescriptionCollection SortDescriptions => throw new NotImplementedException();

        public IEnumerable SourceCollection => throw new NotImplementedException();

        public event EventHandler CurrentChanged;
        public event CurrentChangingEventHandler CurrentChanging;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool Contains(object item)
        {
            throw new NotImplementedException();
        }

        public IDisposable DeferRefresh()
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool MoveCurrentTo(object item)
        {
            throw new NotImplementedException();
        }

        public bool MoveCurrentToFirst()
        {
            throw new NotImplementedException();
        }

        public bool MoveCurrentToLast()
        {
            throw new NotImplementedException();
        }

        public bool MoveCurrentToNext()
        {
            throw new NotImplementedException();
        }

        public bool MoveCurrentToPosition(int position)
        {
            throw new NotImplementedException();
        }

        public bool MoveCurrentToPrevious()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }
    }
}
