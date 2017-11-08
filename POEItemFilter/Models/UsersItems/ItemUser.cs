﻿using POEItemFilter.Library.Enumerables;

namespace POEItemFilter.Models
{
    public class ItemUser
    {
        public int Id { get; set; }

        //public int FilterId { get; set; }

        //public Filter Filter { get; set; }

        /// <summary>
        /// eg. Armour.
        /// </summary>
        public BaseTypes? MainCategory { get; set; }

        /// <summary>
        /// eg. Boot.
        /// </summary>
        public Types? Class { get; set; }

        /// <summary>
        /// One of three: strength, dexterity, intelligence.
        /// </summary>
        public Attributes? Attribute1 { get; set; }

        /// <summary>
        /// One of three: strength, dexterity, intelligence.
        /// </summary>
        public Attributes? Attribute2 { get; set; }

        /// <summary>
        /// eg. Iron Hat.
        /// </summary>
        public string BaseType { get; set; }

        /// <summary>
        /// Minimum level item starts to drop.
        /// </summary>
        public string DropLevel { get; set; }

        /// <summary>
        /// One of four: normal, magic, rare, unique.
        /// </summary>
        public string Rarity { get; set; }

        /// <summary>
        /// Value range: 0-20.
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// Value range: 0-100.
        /// </summary>
        public string ItemLevel { get; set; }

        /// <summary>
        /// Value range: 0-6.
        /// </summary>
        public string Sockets { get; set; }

        /// <summary>
        /// Number of red/green/blue sockets. Values: 0-6.
        /// </summary>
        public string SocketsGroup { get; set; }

        /// <summary>
        /// Number of linked sockets. Values: 0, 2-6.
        /// </summary>
        public string LinkedSockets { get; set; }

        /// <summary>
        /// Value range: 1-4. 
        /// </summary>
        public string Height { get; set; }

        /// <summary>
        /// Value range: 1-2.
        /// </summary>
        public string Width { get; set; }

        /// <summary>
        /// RGB format. Value range: 0-255.
        /// </summary>
        public string SetBorderColor { get; set; }

        /// <summary>
        /// RGB format. Value range: 0-255.
        /// </summary>
        public string SetTextColor { get; set; }

        /// <summary>
        /// RGB format. Value range: 0-255.
        /// </summary>
        public string SetBackgroundColor { get; set; }

        /// <summary>
        /// Value range: 18-45. Default: 32.
        /// </summary>
        public string SetFontSize { get; set; } = "32";

        /// <summary>
        /// Default: false.
        /// </summary>
        public bool Identified { get; set; } = false;

        /// <summary>
        /// Default: false.
        /// </summary>
        public bool Corrupted { get; set; } = false;

        /// <summary>
        /// Default: true.
        /// </summary>
        public bool Show { get; set; } = true;
    }
}