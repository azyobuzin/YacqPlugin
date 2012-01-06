﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using Dulcet.Twitter;
using Inscribe.Core;
using Inscribe.Filter;
using Inscribe.Filter.Core;
using XSpect.Yacq;

namespace YacqPlugin
{
    [Export(typeof(IFilter))]
    public partial class YacqFilter : FilterBase
    {
        private Lazy<Func<TwitterStatusBase, bool>> _compiled;

        private String _query;

        public override bool IsOnlyTranscender
        {
            get
            {
                return true;
            }
        }

        protected override bool FilterStatus(TwitterStatusBase status)
        {
            return this._compiled.Value(status);
        }

        public override string Identifier
        {
            get
            {
                return "y";
            }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return this.Query;
        }

        public override string Description
        {
            get
            {
                return "YACQ クエリによるフィルタ";
            }
        }

        public override string FilterStateString
        {
            get
            {
                return "YACQ クエリ " + this.Query + " でフィルタ";
            }
        }

        [GuiVisible("クエリ")]
        public string Query
        {
            get
            {
                return this._query;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = "true";
                }
                if (this._query == value)
                {
                    return;
                }
                this._query = value;
                this._compiled = new Lazy<Func<TwitterStatusBase, bool>>(() =>
                    YacqServices.ParseFunc<TwitterStatusBase, bool>(new SymbolTable(typeof(Symbols)), this.Query).Compile(),
                    true
                );
                RaiseRequireReaccept();
            }
        }

        public YacqFilter()
            : this("")
        {
        }

        public YacqFilter(string query)
        {
            this.Query = query;
        }
    }
}