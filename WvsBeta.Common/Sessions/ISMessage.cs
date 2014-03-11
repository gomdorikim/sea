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

        ChangeRates,
        RequestBuddylist,
        
        BuddyOperation,
        BuddyInvite,
        BuddyInviteAnswer,
        AdminMessage,
        FindPlayer,

        BuddyDisconnect,
        PartyOperation,

        PlayerUpdateMap, //Used for parties :/
        Test,
        MessengerOperation,
        PartyDisconnect,
        PlayerBuffUpdate,

        Buddychat
    }

    public enum ISServerMessages : byte
    {
        ServerAssignmentResult,

        PlayerChangeServerResult,
        PlayerRequestWorldLoadResult,
        PlayerRequestChannelStatusResult,
        PlayerRequestWorldListResult,
        PlayerWhisperOrFindOperationResult,
        PlayerPartyOperation,
        PlayerSuperMegaphone,

        PlayerSendPacket,

        ChangeRates,
        RequestBuddylist,
        
        BuddyOperation,
        BuddyInvite,
        BuddyInviteAnswer,
        AdminMessage,
        FindPlayer,

        BuddyDisconnect,
        PartyOperation,
        Test,
        MessengerOperation,
        PartyDisconnect,
        PlayerBuffUpdate,
        Buddychat
    }
}
