﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace AntDesign
{
    public partial class Selection : ColumnBase, ISelectionColumn
    {
        [Parameter] public string Type { get; set; } = "checkbox";

        [Parameter] public bool Disabled { get; set; }

        [Parameter] public string Key { get; set; }

        [CascadingParameter(Name = "RowIndex")]
        public int RowIndex { get; set; }

        bool ISelectionColumn.Checked
        {
            get => _checked;
            set => _checked = value;
        }

        private bool _checked;

        private bool Indeterminate => IsHeader
                                      && this.RowSelections.Any(x => x.Checked)
                                      && !this.RowSelections.All(x => x.Checked);

        public IList<ISelectionColumn> RowSelections { get; set; } = new List<ISelectionColumn>();

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (Table != null)
            {
                if (IsHeader)
                {
                    Table.HeaderSelection = this;
                }
                else
                {
                    Table?.HeaderSelection?.RowSelections.Add(this);
                }
            }
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (IsHeader && Type == "radio" && RowSelections.Count(x => x.Checked) > 1)
            {
                var first = RowSelections.FirstOrDefault(x => x.Checked);
                if (first != null)
                {
                    Table?.HeaderSelection.RowSelections.Where(x => x.Index != first.Index).ForEach(x => x.Check(false));
                }
            }
        }

        private void HandleCheckedChange(bool @checked)
        {
            this._checked = @checked;

            if (this.IsHeader)
            {
                RowSelections.Where(x => !x.Disabled).ForEach(x => x.Check(@checked));
            }
            else
            {
                if (Type == "radio")
                {
                    Table?.HeaderSelection.RowSelections.Where(x => x.Index != this.Index).ForEach(x => x.Check(false));
                }

                Table?.HeaderSelection.Check(@checked);
            }

            InvokeSelectedRowsChange();
        }

        void ISelectionColumn.Check(bool @checked)
        {
            this._checked = @checked;
            StateHasChanged();

            InvokeSelectedRowsChange();
        }

        private void InvokeSelectedRowsChange()
        {
            if (IsHeader)
            {
                var checkedIndex = RowSelections.Where(x => x.Checked).Select(x => x.RowIndex - 1).ToArray();
                Table.SelectionChanged(checkedIndex);
            }
        }
    }
}
