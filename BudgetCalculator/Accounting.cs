﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetCalculator
{
    internal class Period
    {
        public Period(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException();
            }
            StartDate = startDate;
            EndDate = endDate;
        }

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
    }

    internal class Accounting
    {
        private readonly IRepository<Budget> _repo;

        public Accounting(IRepository<Budget> repo)
        {
            _repo = repo;
        }

        public decimal TotalAmount(DateTime start, DateTime end)
        {
            var period = new Period(start, end);

            return IsSameMonth(period)
                ? GetOneMonthAmount(period)
                : GetRangeMonthAmount(start, end);
        }

        private decimal GetRangeMonthAmount(DateTime start, DateTime end)
        {
            var monthCount = end.MonthDifference(start);
            var total = 0;
            for (var index = 0; index <= monthCount; index++)
            {
                if (index == 0)
                {
                    var period = new Period(start, start.LastDate());
                    total += GetOneMonthAmount(period);
                }
                else if (index == monthCount)
                {
                    var period = new Period(end.FirstDate(), end);
                    total += GetOneMonthAmount(period);
                }
                else
                {
                    var now = start.AddMonths(index);
                    var period = new Period(now.FirstDate(), now.LastDate());
                    total += GetOneMonthAmount(period);
                }
            }
            return total;
        }

        private bool IsSameMonth(Period period)
        {
            return period.StartDate.Year == period.EndDate.Year && period.StartDate.Month == period.EndDate.Month;
        }

        private int GetOneMonthAmount(Period period)
        {
            var list = this._repo.GetAll();
            var budget = list.Get(period.StartDate)?.Amount ?? 0;

            var days = DateTime.DaysInMonth(period.StartDate.Year, period.StartDate.Month);
            var validDays = GetValidDays(period.StartDate, period.EndDate);

            return (budget / days) * validDays;
        }

        private int GetValidDays(DateTime start, DateTime end)
        {
            return (end - start).Days + 1;
        }
    }

    public static class BudgetExtension
    {
        public static Budget Get(this List<Budget> list, DateTime date)
        {
            return list.FirstOrDefault(r => r.YearMonth == date.ToString("yyyyMM"));
        }
    }

    public static class DateTimeExtension
    {
        public static int MonthDifference(this DateTime lValue, DateTime rValue)
        {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }

        public static DateTime LastDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        public static DateTime FirstDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
    }
}