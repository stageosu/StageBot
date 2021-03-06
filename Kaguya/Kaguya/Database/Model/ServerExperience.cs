﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kaguya.Database.Interfaces;

namespace Kaguya.Database.Model
{
    public class ServerExperience : IUserSearchable, IServerSearchable
    {
        [Key][Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ServerId { get; set; }
        [Key][Column(Order = 1)]
        public ulong UserId { get; set; }
        public int Exp { get; private set; }
        public DateTimeOffset? LastGivenExp { get; set; }

        /// <summary>
        /// Adds the absolute value of <see cref="amount"/> to the current object's <see cref="Exp"/> value.
        /// </summary>
        /// <param name="amount"></param>
        public void AddExp(int amount)
        {
            this.Exp += Math.Abs(amount);
            this.LastGivenExp = DateTimeOffset.Now;
        }

        /// <summary>
        /// Subtracts the absolute value of <see cref="amount"/> to the current object's <see cref="Exp"/> value.
        /// </summary>
        /// <param name="amount"></param>
        public void SubtractExp(int amount)
        {
            if ((Math.Abs(amount) - this.Exp) < 0)
            {
                this.Exp = 0;
            }
            else
            {
                this.Exp -= Math.Abs(amount);  
            }
        } 
    }
}