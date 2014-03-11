using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WvsBeta.Common.Sessions
{
    public enum ISClientMessages : byte
    {
        ServerRequestAllocation,
        ServerSetConnectionsValue,
        ServerRegisterUnregisterPlayer,

        PlayerChangeServer,
        PlayerQuitCashShop,
        PlayerRequestWorldLoad,
        PlayerRequestWorldList,
        PlayerRequestChannelStatus,
        PlayerWhisperOrFindOperation,
        PlayerPartyOperation,
        PlayerUsingSuperMegaphone,

        MessengerJoin,
        MessengerLeave,
        MessengerInvite,
        MessengerBlocked,
        MessengerChat,

        BuddyInvite,
        BuddyInviteAnswer,
    }

    public enum ISServerMessages : byte
    {
        ServerAssignmentResult,

        PlayerChangeServerResult,
        PlayerRequestWorldLoadResult,
        PlayerRequestChannelStatusResult,
        PlayerRequestWorldListResult,
        PlayerWhisperOrFindOperationResult,
        PlayerPartyOperationResult,
        PlayerSuperMegaphone,

        PlayerSendPacket,

        BuddyInvite,
        BuddyInviteAnswer,
    }
}
