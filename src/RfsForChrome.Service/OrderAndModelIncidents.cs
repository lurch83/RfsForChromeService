﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RfsForChrome.Service.Extensions;
using RfsForChrome.Service.Models;

namespace RfsForChrome.Service
{
    public interface IOrderAndModelIncidents
    {
        IEnumerable<Incident> CreateModel(XDocument document);
    }

    public class OrderAndModelIncidents : IOrderAndModelIncidents
    {
        public IEnumerable<Incident> CreateModel(XDocument document)
        {
            var items = document.Descendants("item");

            var incidents = items.Select(s => new Incident()
                {
                    Title = s.Element("title").Value,
                    Category = GetCategory(s.Element("category").Value),
                    LastUpdated = ParseLastUpdatedFromDescriptionString(s.Element("description").Value).MyToDateTime(),
                    CouncilArea = ParseItemFromDescriptionString(s.Element("description").Value, "COUNCIL AREA"),
                    Status = ParseItemFromDescriptionString(s.Element("description").Value, "STATUS"),
                    Location = ParseItemFromDescriptionString(s.Element("description").Value, "LOCATION"),
                    Size = ParseItemFromDescriptionString(s.Element("description").Value, "SIZE"),
                    Type = ParseItemFromDescriptionString(s.Element("description").Value, "TYPE"),
                    Link = s.Element("link").Value,
                    MajorFireUpdate = ParseItemFromDescriptionString(s.Element("description").Value, "MAJOR FIRE UPDATE"),

                }).ToList();
            //incidents.Add(new Incident()
            //    {
            //        Category = Category.EmergencyWarning,
            //        Title = "Some Dodgy Fire",
            //        Status = "out of control",
            //        LastUpdated = DateTime.Now,
            //        Location = "Somewhere",
            //        Size = "121 ha",
            //        CouncilArea = "Cessnock",
            //        Type = "Hazard Reduction"
            //    });
            //incidents.Add(new Incident()
            //{
            //    Category = Category.WatchAndAct,
            //    Title = "Another Dodgy Fire",
            //    Status = "out of control",
            //    LastUpdated = DateTime.Now,
            //    Location = "Somewhere",
            //    Size = "1 ha",
            //    CouncilArea = "Cessnock",
            //    Type = "Hazard Reduction"
            //});
            return incidents.OrderBy(incident => incident.Category).ThenByDescending(incident => incident.LastUpdated);
        }

        private string ParseItemFromDescriptionString(string value, string item)
        {
            if (value.IndexOf(item) == -1)
                return string.Empty;
            var trimmed = value.Trim().Replace("\n", " ");
            string regex = string.Format("<br />{0}: (.*?)<br />",item);
            return Regex.Split(trimmed, regex).ElementAt(1).Trim();
        }

        private string ParseLastUpdatedFromDescriptionString(string value)
        {
            var trimmed = value.Trim();
            string regex = string.Format("<br />UPDATED: (.*?)");
            return Regex.Split(trimmed, regex).ElementAt(2).Trim();
        }

        private Category GetCategory(string value)
        {
            switch (value)
            {
                case "Emergency Warning":
                    return Category.EmergencyWarning;
                case "Watch and Act":
                    return Category.WatchAndAct;
                case "Advice":
                    return Category.Advice;
                default:
                    return Category.NotApplicable;
            }
        }
    }
}