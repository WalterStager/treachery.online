﻿/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class GlobalChatMessage : ChatMessage
{
    public override Message GetBodyIncludingPlayerInfo(int receivingUserId, Game game, GameParticipation participation, bool contextIsGlobal)
    {
        if (contextIsGlobal)
        {
            return SourceUserId == receivingUserId ? 
                Message.Express("You: ", Body) : 
                Message.Express(participation.GetPlayerName(SourceUserId), ": ", Body);
        }

        return SourceUserId == receivingUserId ? 
            Message.Express("You: ", Body, " ⇒ GLOBAL") : 
            Message.Express(participation.GetPlayerName(SourceUserId), ": ", Body, " ⇒ GLOBAL");
    }
}