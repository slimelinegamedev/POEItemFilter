﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using POEItemFilter.Library;
using POEItemFilter.Models;
using POEItemFilter.Models.ItemsDB;
using POEItemFilter.ViewModels;

namespace POEItemFilter.Controllers
{
    [Authorize]
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

        [HttpGet]
        public ActionResult ItemSession(int? id)
        {
            if (id == null)
            {
                return View("ItemSession", new ItemUserViewModel());
            }

            List<ItemUser> viewModel = Session["ItemsList"] as List<ItemUser>;
            if (viewModel == null)
            {
                return HttpNotFound();
            }

            var item = ItemUserModelMap.ItemUserToViewModel(viewModel.SingleOrDefault(i => i.Id == id));
            if (item == null)
            {
                return HttpNotFound();
            }

            return View("ItemSession", item);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SaveItemSession(ItemUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("ItemSession", "UsersItems");
            }

            ItemUser item = ItemUserModelMap.ViewModelToItemUser(model);

            List<ItemUser> sessionModel = Session["ItemsList"] as List<ItemUser>;
            if (sessionModel == null)
            {
                item.Id = 1;
                Session["ItemsList"] = new List<ItemUser>();
                Session.Timeout = 30;
            }
            else if (item.Id != 0)
            {
                int index = sessionModel.FindIndex(i => i.Id == item.Id);
                sessionModel[index] = item;
                return RedirectToAction("New", "Filters");
            }
            else
            {
                item.Id = sessionModel.Count > 0 ? sessionModel.Max(i => i.Id) + 1 : 1;
            }

            List<ItemUser> viewModel = Session["ItemsList"] as List<ItemUser>;
            viewModel.Add(item);

            return RedirectToAction("New", "Filters");
        }

        [HttpPost]
        public ActionResult DeleteItemSession(int id)
        {
            List<ItemUser> viewModel = Session["ItemsList"] as List<ItemUser>;
            var item = viewModel.SingleOrDefault(i => i.Id == id);
            if (item == null)
            {
                return View("New", "Filters");
            }
            viewModel.Remove(item);
            Session["ItemsList"] = viewModel;

            return RedirectToAction("New", "Filters");
        }

        [HttpGet]
        public ActionResult ItemDb(int? filterId, int? itemId)
        {
            bool isAuthorized = false;

            if (filterId != null)
            {
                isAuthorized = User.Identity.GetUserId() == _context.Filters.SingleOrDefault(i => i.Id == filterId).UserId;
                if (!isAuthorized)
                {
                    return HttpNotFound();
                }
                return View("ItemDb", new ItemUserViewModel() { FilterId = filterId });
            }

            if (itemId != null)
            {
                var itemInDb = _context.UsersItems.SingleOrDefault(i => i.Id == itemId);
                if (itemInDb == null)
                {
                    return HttpNotFound();
                }
                isAuthorized = User.Identity.GetUserId() == _context.Filters.SingleOrDefault(i => i.Id == itemInDb.FilterId).UserId;
                if (!isAuthorized)
                {
                    return HttpNotFound();
                }
                ItemUserViewModel viewModel = ItemUserModelMap.ItemUserToViewModel(itemInDb);
                return View("ItemDb", viewModel);
            }

            return View("MyFilters", "Filters");

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SaveItemDb(ItemUserViewModel model)
        {
            bool isAuthorized = User.Identity.GetUserId() == _context.Filters.SingleOrDefault(i => i.Id == model.FilterId).UserId;
            if (!isAuthorized)
            {
                return HttpNotFound();
            }
            if (!ModelState.IsValid)
            {
                return RedirectToAction("MyFilters", "UsersItems");
            }

            ItemUser item = ItemUserModelMap.ViewModelToItemUser(model);

            if (model.ItemId == null)
            {
                int lastRowId = _context.Filters
                    .SelectMany(i => i.Items)
                    .Where(i => i.FilterId == item.FilterId)
                    .Max(i => i.RowId);

                item.RowId = lastRowId + 1;
                _context.UsersItems.Add(item);
            }
            else
            {
                ItemUser itemInDb = new ItemUser();
                itemInDb = _context.UsersItems.SingleOrDefault(i => i.Id == item.Id);
                itemInDb.BaseType = item.BaseType;
                itemInDb.Attribute1 = item.Attribute1;
                itemInDb.Attribute2 = item.Attribute2;
                itemInDb.Class = item.Class;
                itemInDb.Corrupted = item.Corrupted;
                itemInDb.DropLevel = item.DropLevel;
                itemInDb.Height = item.Height;
                itemInDb.Identified = item.Identified;
                itemInDb.ItemLevel = item.ItemLevel;
                itemInDb.LinkedSockets = item.LinkedSockets;
                itemInDb.MainCategory = item.MainCategory;
                itemInDb.PlayAlertSound = item.PlayAlertSound;
                itemInDb.Quality = item.Quality;
                itemInDb.Rarity = item.Rarity;
                itemInDb.SetBackgroundColor = item.SetBackgroundColor;
                itemInDb.SetBorderColor = item.SetBorderColor;
                itemInDb.SetFontSize = item.SetFontSize;
                itemInDb.SetTextColor = item.SetTextColor;
                itemInDb.Show = item.Show;
                itemInDb.Sockets = item.Sockets;
                itemInDb.SocketsGroup = item.SocketsGroup;
                itemInDb.Width = item.Width;
                itemInDb.UserBaseType = item.UserBaseType;
            }
            _context.SaveChanges();
            return RedirectToAction("Edit", "Filters", new { id = item.FilterId });
        }

        [HttpPost]
        public ActionResult DeleteItemDb(int id)
        {
            var itemInDb = _context.UsersItems.SingleOrDefault(i => i.Id == id);
            int filterId = itemInDb.FilterId;
            bool isAuthorized = User.Identity.GetUserId() == _context.Filters.SingleOrDefault(i => i.Id == filterId).UserId;
            if (!isAuthorized)
            {
                return HttpNotFound();
            }
            if (itemInDb != null)
            {
                _context.UsersItems.Remove(itemInDb);
                _context.SaveChanges();
            }

            return RedirectToAction("Edit", "Filters", new { id = filterId });
        }

        /// <summary>
        /// The method is filtering database depending on received data.
        /// </summary>
        /// <param name="id">Represents data entered by the user. Format: baseType|type|attribute1|attribute2. If user don't select one of the parameters, then it's null.</param>
        /// <returns>Refresh view with new data.</returns>
        [HttpGet]
        public ActionResult Refresh(int? baseType, int? type, int? attribute1, int? attribute2)
        {
            var baseTypes = _context.BaseTypes.ToList();

            var itemsList = _context.ItemsDB.ToList();

            int baseTypeId, typeId, attribute1Id, attribute2Id;

            baseTypeId = baseType != null ? baseType.Value : -1;
            typeId = type != null ? type.Value : -1;
            attribute1Id = attribute1 != null ? attribute1.Value : -1;
            attribute2Id = attribute2 != null ? attribute2.Value : -1;

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
            };

            return PartialView("_RefreshPartial", viewModel);
        }
    }
}