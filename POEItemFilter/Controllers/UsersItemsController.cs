﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using POEItemFilter.Library;
using POEItemFilter.Library.Enumerables;
using POEItemFilter.Models;
using POEItemFilter.Models.ItemsDB;
using POEItemFilter.ViewModels;

namespace POEItemFilter.Controllers
{
    public class UsersItemsController : Controller
    {
        ApplicationDbContext _context;

        public UsersItemsController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        // GET: AllItems
        public ActionResult NewItem()
        {
            var baseTypes = _context.BaseTypes.ToList();
            var items = baseTypes.SelectMany(i => i.Items).ToList();
            var types = baseTypes.SelectMany(i => i.Types).ToList();
            var attributes = baseTypes.SelectMany(i => i.Attributes).ToList();

            var viewModel = new NewItemViewModel()
            {
                Items = items,
                BaseTypes = baseTypes,
                Types = types,
                Attributes = attributes
            };

            return View(viewModel);
        }

        /// <summary>
        /// The method is filtering database depending on received data.
        /// </summary>
        /// <param name="id">Represents data entered by the user. Format: baseType|type|attribute1|attribute2. If user don't select one of the parameters, then it's null.</param>
        /// <returns>Refresh view with new data.</returns>
        public ActionResult Refresh(string id)
        {
            var baseTypes = _context.BaseTypes.ToList();

            var itemsList = baseTypes
                .SelectMany(i => i.Items)
                .ToList();

            string[] ids = id.Split('|');
            int baseTypeId, typeId, attribute1Id, attribute2Id, lastBaseTypeId, lastTypeId, lastAttributeId, lastItemId;

            lastBaseTypeId = baseTypes.LastOrDefault().Id + 1;
            lastTypeId = baseTypes.SelectMany(i => i.Types).LastOrDefault().Id + 1;
            lastAttributeId = baseTypes.SelectMany(i => i.Attributes).LastOrDefault().Id + 1;
            lastItemId = itemsList.LastOrDefault().Id + 1;

            if (!int.TryParse(ids[0], out baseTypeId))
            {
                baseTypeId = baseTypes.Count() + 1;
            }

            if (!int.TryParse(ids[1], out typeId))
            {
                typeId = baseTypes.SelectMany(i => i.Types).Count() + 1;
            }

            if (!int.TryParse(ids[2], out attribute1Id))
            {
                attribute1Id = baseTypes.SelectMany(i => i.Attributes).Count() + 1;
            }

            if (!int.TryParse(ids[3], out attribute2Id))
            {
                attribute2Id = baseTypes.SelectMany(i => i.Attributes).Count() + 1;
            }

            bool isBaseTypeInDb = baseTypes.Any(i => i.Id == baseTypeId);

            bool isTypeIdInDb = baseTypes
                .SelectMany(i => i.Types)
                .Any(i => i.Id == typeId);

            var types = (List<ItemType>)null;

            if (isTypeIdInDb)
            {
                if (isBaseTypeInDb)
                {
                    types = baseTypes
                        .Where(i => i.Id == baseTypeId)
                        .SelectMany(i => i.Types)
                        .ToList();
                }
                else
                {
                    types = baseTypes
                        .SelectMany(i => i.Types)
                        .ToList();
                }
            }
            else
            {
                types = baseTypes
                    .SelectMany(i => i.Types)
                    .Where(i => i.BaseTypeId == baseTypeId)
                    .ToList();

                if (types.Count() == 0)
                {
                    types = baseTypes
                    .SelectMany(i => i.Types)
                    .ToList();
                }
            }

            bool isBaseTypeArmour = baseTypes.SelectMany(i => i.Attributes).Any(i => i.BaseTypeId == baseTypeId);
            bool isTypeArmour = baseTypes.Where(i => i.Name == "Armour").SelectMany(i => i.Types).Any(i => i.Id == typeId);

            var attributes = (List<ItemAttribute>)null;
            var items = (List<ItemDB>)null;

            // This part of code applies only to items with attributes (armour and it's types)
            if (isBaseTypeArmour || isTypeArmour || !isBaseTypeInDb && !isTypeIdInDb)
            {
                bool isAttr1InDb = baseTypes
                    .SelectMany(i => i.Attributes)
                    .Any(i => i.Id == attribute1Id);

                bool isAttr2InDb = baseTypes
                    .SelectMany(i => i.Attributes)
                    .Any(i => i.Id == attribute2Id);

                attributes = baseTypes
                    .SelectMany(i => i.Attributes)
                    .ToList();

                if (isAttr1InDb && !isAttr2InDb)
                {
                    var firstFilter = attributes
                        .Where(i => i.Id == attribute1Id)
                        .SelectMany(i => i.Items)
                        .ToList();

                    var secondFilter = attributes
                        .Where(i => i.Id != attribute1Id)
                        .SelectMany(i => i.Items)
                        .ToList();

                    items = new List<ItemDB>();
                    foreach (var item in firstFilter)
                    {
                        bool match = secondFilter.Any(i => i.Id == item.Id);
                        if (!match)
                        {
                            items.Add(item);
                        }
                    }
                }

                else if (!isAttr1InDb && isAttr2InDb)
                {
                    var firstFilter = attributes
                        .Where(i => i.Id == attribute2Id)
                        .SelectMany(i => i.Items)
                        .ToList();

                    var secondFilter = attributes
                        .Where(i => i.Id != attribute2Id)
                        .SelectMany(i => i.Items)
                        .ToList();

                    items = new List<ItemDB>();
                    foreach (var item in firstFilter)
                    {
                        bool match = secondFilter.Any(i => i.Id == item.Id);
                        if (!match)
                        {
                            items.Add(item);
                        }
                    }
                }

                else if (isAttr1InDb && isAttr2InDb)
                {
                    if (attribute1Id != attribute2Id)
                    {
                        var firstFilter = attributes
                        .Where(i => i.Id == attribute1Id)
                        .SelectMany(i => i.Items)
                        .ToList();

                        var secondFilter = attributes
                            .Where(i => i.Id == attribute2Id)
                            .SelectMany(i => i.Items)
                            .ToList();

                        items = firstFilter
                            .Join(secondFilter,
                             a => a.Id,
                             b => b.Id,
                            (a, b) => new ItemDB()
                            {
                                Attributes = b.Attributes,
                                BaseType = b.BaseType,
                                BaseTypeId = b.BaseTypeId,
                                Id = b.Id,
                                Level = b.Level,
                                Name = b.Name,
                                Type = b.Type,
                                TypeId = b.TypeId
                            })
                            .ToList();

                        // Alternative
                        //items = (from a in firstFilter
                        //         join b in secondFilter
                        //         on a.Id
                        //         equals b.Id
                        //         select new ItemDB()
                        //         {
                        //             Attributes = b.Attributes,
                        //             BaseType = b.BaseType,
                        //             BaseTypeId = b.BaseTypeId,
                        //             Id = b.Id,
                        //             Level = b.Level,
                        //             Name = b.Name,
                        //             Type = b.Type,
                        //             TypeId = b.TypeId
                        //         }).ToList();
                    }
                    else
                    {
                        var firstFilter = attributes
                        .Where(i => i.Id == attribute1Id)
                        .SelectMany(i => i.Items)
                        .ToList();

                        var secondFilter = attributes
                            .Where(i => i.Id != attribute2Id)
                            .SelectMany(i => i.Items)
                            .ToList();

                        items = new List<ItemDB>();
                        foreach (var item in firstFilter)
                        {
                            bool match = secondFilter.Any(i => i.Id == item.Id);
                            if (!match)
                            {
                                items.Add(item);
                            }
                        }
                    }
                }

                if (isTypeIdInDb && items != null)
                {
                    items = items.Where(i => i.TypeId == typeId).ToList();
                }
                else if (isBaseTypeInDb && items != null)
                {
                    items = items.Where(i => i.BaseTypeId == baseTypeId).ToList();
                }
                else if (isTypeIdInDb)
                {
                    items = itemsList
                        .Where(i => i.TypeId == typeId)
                        .Select(i => i)
                        .ToList();
                }
                else if (isBaseTypeInDb)
                {
                    items = itemsList
                        .Where(i => i.BaseTypeId == baseTypeId)
                        .Select(i => i)
                        .ToList();
                }
                else if (attributes.Any(i => i.Id == attribute1Id) || attributes.Any(i => i.Id == attribute2Id))
                {

                }
                else
                {
                    items = itemsList;
                }
            }

            // If item doesn't have attribute.
            else
            {
                if (isTypeIdInDb)
                {
                    items = itemsList
                        .Where(i => i.TypeId == typeId)
                        .Select(i => i)
                        .ToList();
                }
                else if (isBaseTypeInDb)
                {
                    items = itemsList
                        .Where(i => i.BaseTypeId == baseTypeId)
                        .Select(i => i)
                        .ToList();
                }
                else
                {
                    items = itemsList;
                }
            }

            var viewModel = new NewItemViewModel()
            {
                BaseTypes = baseTypes,
                Types = types,
                Attributes = attributes,
                Items = items,
                LastBaseTypeId = lastBaseTypeId,
                LastAttributeId = lastAttributeId,
                LastItemId = lastItemId,
                LastTypeId = lastTypeId
            };

            return View("Refresh", viewModel);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SaveItem(string id)
        {
            string[] splitId = id.Split('|');
            ItemUser item = new ItemUser();

            for (int i = 0; i < splitId.Length; i++)
            {
                int number = -1;
                if (splitId[i] != "undefined")
                {
                    number = int.Parse(splitId[i].ToString());
                }
                switch (i)
                {
                    case 0:
                        item.MainCategory = _context.BaseTypes.Where(a => a.Id == number).Select(a => a.Name).SingleOrDefault();
                        break;

                    case 1:
                        item.Class = _context.Types.Where(a => a.Id == number).Select(a => a.Name).SingleOrDefault();
                        break;

                    case 2:
                        item.Attribute1 = _context.Attributes.Where(a => a.Id == number).Select(a => a.Name).SingleOrDefault();
                        break;

                    case 3:
                        item.Attribute2 = _context.Attributes.Where(a => a.Id == number).Select(a => a.Name).SingleOrDefault();
                        break;

                    case 4:
                        item.ItemLevel = InequalitySelector.Select(number);
                        break;

                    case 5:
                        if (item.ItemLevel != null)
                        {
                            item.ItemLevel += " " + number;
                        }
                        break;

                    case 6:
                        item.Quality = InequalitySelector.Select(number);
                        break;

                    case 7:
                        if (item.Quality != null)
                        {
                            item.Quality += " " + number;
                        }
                        break;

                    case 8:
                        item.Rarity = InequalitySelector.Select(number);
                        break;

                    case 9:
                        if (item.Rarity != null)
                        {
                            item.Rarity += " " + (Rarity)number;
                        }
                        break;

                    case 10:
                        item.Sockets = InequalitySelector.Select(number);
                        break;

                    case 11:
                        if (item.Sockets != null)
                        {
                            item.Sockets += " " + number;
                        }
                        break;

                    case 12:
                        item.LinkedSockets = InequalitySelector.Select(number);
                        break;

                    case 13:
                        if (item.LinkedSockets != null)
                        {
                            item.LinkedSockets += " " + number;
                        }
                        break;

                    case 14:
                        if (number != 300)
                        {
                            item.SocketsGroup = string.Concat(Enumerable.Repeat("R", number));
                        }
                        break;

                    case 15:
                        if (number != 300)
                        {
                            item.SocketsGroup += string.Concat(Enumerable.Repeat("G", number));
                        }
                        break;

                    case 16:
                        if (number != 300)
                        {
                            item.SocketsGroup += string.Concat(Enumerable.Repeat("B", number));
                        }
                        break;

                    case 17:
                        if (number != 300)
                        {
                            item.SocketsGroup += string.Concat(Enumerable.Repeat("W", number));
                        }
                        break;

                    case 18:
                        item.Height = InequalitySelector.Select(number);
                        break;

                    case 19:
                        if (item.Height != null)
                        {
                            item.Height += " " + number;
                        }
                        break;

                    case 20:
                        item.Width = InequalitySelector.Select(number);
                        break;

                    case 21:
                        if (item.Width != null)
                        {
                            item.Width += " " + number;
                        }
                        break;

                    case 22:
                        item.Identified = Convert.ToBoolean(number);
                        break;

                    case 23:
                        item.Corrupted = Convert.ToBoolean(number);
                        break;

                    case 24:
                        if (number != 300)
                        {
                            item.SetBorderColor = number.ToString();

                        }
                        break;

                    case 25:
                        if (number != 300)
                        {
                            item.SetBorderColor += " " + number.ToString();
                        }
                        break;

                    case 26:
                        if (number != 300)
                        {
                            item.SetBorderColor += " " + number.ToString();
                        }
                        break;

                    case 27:
                        if (number != 300)
                        {
                            item.SetTextColor = number.ToString();
                        }
                        break;

                    case 28:
                        if (number != 300)
                        {
                            item.SetTextColor += " " + number.ToString();
                        }
                        break;

                    case 29:
                        if (number != 300)
                        {
                            item.SetTextColor += " " + number.ToString();
                        }
                        break;

                    case 30:
                        if (number != 300)
                        {
                            item.SetBackgroundColor = number.ToString();
                        }
                        break;

                    case 31:
                        if (number != 300)
                        {
                            item.SetBackgroundColor += " " + number.ToString();
                        }
                        break;

                    case 32:
                        if (number != 300)
                        {
                            item.SetBackgroundColor += " " + number.ToString();
                        }
                        break;

                    case 33:
                        if (number != 300)
                        {
                            item.SetFontSize = number.ToString();
                        }
                        break;

                    case 34:
                        item.BaseType = _context.ItemsDB.Where(a => a.Id == number).Select(a => a.Name).SingleOrDefault();
                        break;
                }
            }

            return Json(Url.Action("AddItem", "Filters", item));
        }
    }
}