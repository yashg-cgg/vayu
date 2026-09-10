using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Vayu.PowerMap
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class TreeNode : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode"/> class.
        /// </summary>
        public TreeNode()
        {
            this.id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// The children
        /// </summary>
        private ObservableCollection<TreeNode> children = new ObservableCollection<TreeNode>();
        /// <summary>
        /// The parent
        /// </summary>
        private ObservableCollection<TreeNode> parent = new ObservableCollection<TreeNode>();
        /// <summary>
        /// The text
        /// </summary>
        private string text;
        /// <summary>
        /// The identifier
        /// </summary>
        private string id;
        /// <summary>
        /// The is checked
        /// </summary>
        private bool? isChecked = true;
        /// <summary>
        /// The is expanded
        /// </summary>
        private bool isExpanded;
        /// <summary>
        /// The third maplayer
        /// </summary>
        private Third_Layer third_maplayer = new Third_Layer();

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public ObservableCollection<TreeNode> Children
        {
            get { return this.children; }
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ObservableCollection<TreeNode> Parent
        {
            get { return this.parent; }
        }

        /// <summary>
        /// Gets or sets the third layer.
        /// </summary>
        /// <value>
        /// The third layer.
        /// </value>
        public Third_Layer ThirdLayer
        {
            get { return this.third_maplayer; }
            set
            {
                this.third_maplayer = value;
                RaisePropertyChanged("LayerChanged");
            }
        }

        /// <summary>
        /// Gets or sets the is checked.
        /// </summary>
        /// <value>
        /// The is checked.
        /// </value>
        public bool? IsChecked
        {
            get { return this.isChecked; }
            set
            {
                this.isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                RaisePropertyChanged("Text");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id
        {
            get { return this.id; }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            int countCheck = 0;

            if ((bool)this.isChecked)
            {
                third_maplayer.ON_OFF = true;
            }
            else
            {
                third_maplayer.ON_OFF = false;
            }
            if (propertyName == "IsChecked")
            {
                if (this.Id == CheckBoxId.checkBoxId && this.Parent.Count == 0 && this.Children.Count != 0)
                {
                    CheckedTreeParent(this.Children, this.IsChecked);
                }
                if (this.Id == CheckBoxId.checkBoxId && this.Parent.Count > 0 && this.Children.Count > 0)
                {
                    CheckedTreeChildMiddle(this.Parent, this.Children, this.IsChecked);
                }
                if (this.Id == CheckBoxId.checkBoxId && this.Parent.Count > 0 && this.Children.Count == 0)
                {
                    CheckedTreeChild(this.Parent, countCheck);
                }
            }
        }

        /// <summary>
        /// Checkeds the tree child middle.
        /// </summary>
        /// <param name="itemsParent">The items parent.</param>
        /// <param name="itemsChild">The items child.</param>
        /// <param name="isChecked">The is checked.</param>
        private void CheckedTreeChildMiddle(ObservableCollection<TreeNode> itemsParent, ObservableCollection<TreeNode> itemsChild, bool? isChecked)
        {
            int countCheck = 0;
            CheckedTreeParent(itemsChild, isChecked);
            CheckedTreeChild(itemsParent, countCheck);
        }

        /// <summary>
        /// Checkeds the tree parent.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="isChecked">The is checked.</param>
        private void CheckedTreeParent(ObservableCollection<TreeNode> items, bool? isChecked)
        {
            foreach (TreeNode item in items)
            {
                item.IsChecked = isChecked;
                if (item.Children.Count != 0) CheckedTreeParent(item.Children, isChecked);
            }
        }

        /// <summary>
        /// Checkeds the tree child.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="countCheck">The count check.</param>
        private void CheckedTreeChild(ObservableCollection<TreeNode> items, int countCheck)
        {
            bool isNull = false;
            foreach (TreeNode paren in items)
            {
                foreach (TreeNode child in paren.Children)
                {
                    if (child.IsChecked == true || child.IsChecked == null)
                    {
                        countCheck++;
                        if (child.IsChecked == null)
                            isNull = true;
                    }
                }
                if (countCheck != paren.Children.Count && countCheck != 0) paren.IsChecked = null;
                else if (countCheck == 0) paren.IsChecked = false;
                else if (countCheck == paren.Children.Count && isNull) paren.IsChecked = null;
                else if (countCheck == paren.Children.Count && !isNull) paren.IsChecked = true;
                if (paren.Parent.Count != 0) CheckedTreeChild(paren.Parent, 0);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct CheckBoxId
    {
        /// <summary>
        /// The check box identifier
        /// </summary>
        public static string checkBoxId;
    }
}
