﻿using System;
using System.Net;
using System.Windows;
using eHCMS.Services.Core.Base;

namespace DataEntities
{
    public class PharmacySellingPriceListSearchCriteria: SearchCriteriaBase
    {
        public PharmacySellingPriceListSearchCriteria()
        { 
        }

        private string _PriceListTitle;
        public string PriceListTitle
        {
            get { return _PriceListTitle; }
            set
            {
                _PriceListTitle=value;
                RaisePropertyChanged("PriceListTitle");
            }
        }

        private int _Month;
        public int Month
        {
            get { return _Month; }
            set
            {
                _Month = value;
                RaisePropertyChanged("Month");
            }
        }


        private int _Year;
        public int Year
        {
            get { return _Year; }
            set
            {
                _Year = value;
                RaisePropertyChanged("Year");
            }
        }
        

        private string _OrderBy;
        public string OrderBy
        {
            get { return _OrderBy; }
            set 
            { 
                _OrderBy = value;
                RaisePropertyChanged("OrderBy");
            }
        }

    }
}

