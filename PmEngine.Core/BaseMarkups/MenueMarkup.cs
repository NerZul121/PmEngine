﻿using PmEngine.Core.Interfaces;

namespace PmEngine.Core.BaseMarkups
{
    /// <summary>
    /// Markup with 3 another markups in header, body and footer
    /// </summary>
    public class MenueMarkup : INextActionsMarkup
    {
        /// <summary>
        /// InLine мод клавиатуры
        /// </summary>
        public bool InLine { get; set; }

        /// <summary>
        /// Аргументы
        /// </summary>
        public Arguments Arguments { get; set; } = new Arguments();

        /// <summary>
        /// Top actions
        /// </summary>
        public INextActionsMarkup Header { get; set; } = new SingleMarkup();

        /// <summary>
        /// Actions on center lines
        /// </summary>
        public INextActionsMarkup Body { get; set; } = new SingleMarkup();

        /// <summary>
        /// Actions in bot
        /// </summary>
        public INextActionsMarkup Footer { get; set; } = new SingleMarkup();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<ActionWrapper>> GetNextActions()
        {
            var result = new List<IEnumerable<ActionWrapper>>();
            result.AddRange(Header.GetNextActions());
            result.AddRange(Body.GetNextActions());
            result.AddRange(Footer.GetNextActions());
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ActionWrapper> GetFloatNextActions()
        {
            var result = new List<ActionWrapper>();
            result.AddRange(Header.GetFloatNextActions());
            result.AddRange(Body.GetFloatNextActions());
            result.AddRange(Footer.GetFloatNextActions());
            return result;
        }

        /// <summary>
        /// Markup with 3 another markups in header, body and footer
        /// </summary>
        public MenueMarkup()
        {
        }

        /// <summary>
        /// Markup with 3 another markups in header, body and footer
        /// </summary>
        public MenueMarkup(INextActionsMarkup header, INextActionsMarkup body, INextActionsMarkup footer)
        {
            Header = header;
            Body = body;
            Footer = footer;
        }
    }
}