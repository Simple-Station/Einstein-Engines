-- This is a Wireshark Lua plugin to add a Dissector for the Lidgren protocol.
-- In case you ever decide you need to open Lidgren's traffic in Wireshark.
-- 
-- To install this plugin, install it in your Wireshark plugin folder:
-- https://www.wireshark.org/docs/wsug_html_chunked/ChPluginFolders.html
-- You can reload plugins in the UI by going Analyze -> Reload Lua Plugins
-- 
-- Because Lidgren doesn't have fixed port numbers, 
-- you may want to switch the port number at the bottom to whatever your program uses.

lidgren_message_types = {
    [0] = "Unconnected",
    [1] = "UserUnreliable",
    [2] = "UserSequenced1",
    [3] = "UserSequenced2",
    [4] = "UserSequenced3",
    [5] = "UserSequenced4",
    [6] = "UserSequenced5",
    [7] = "UserSequenced6",
    [8] = "UserSequenced7",
    [9] = "UserSequenced8",
    [10] = "UserSequenced9",
    [11] = "UserSequenced10",
    [12] = "UserSequenced11",
    [13] = "UserSequenced12",
    [14] = "UserSequenced13",
    [15] = "UserSequenced14",
    [16] = "UserSequenced15",
    [17] = "UserSequenced16",
    [18] = "UserSequenced17",
    [19] = "UserSequenced18",
    [20] = "UserSequenced19",
    [21] = "UserSequenced20",
    [22] = "UserSequenced21",
    [23] = "UserSequenced22",
    [24] = "UserSequenced23",
    [25] = "UserSequenced24",
    [26] = "UserSequenced25",
    [27] = "UserSequenced26",
    [28] = "UserSequenced27",
    [29] = "UserSequenced28",
    [30] = "UserSequenced29",
    [31] = "UserSequenced30",
    [32] = "UserSequenced31",
    [33] = "UserSequenced32",
    [34] = "UserReliableUnordered",
    [35] = "UserReliableSequenced1",
    [36] = "UserReliableSequenced2",
    [37] = "UserReliableSequenced3",
    [38] = "UserReliableSequenced4",
    [39] = "UserReliableSequenced5",
    [40] = "UserReliableSequenced6",
    [41] = "UserReliableSequenced7",
    [42] = "UserReliableSequenced8",
    [43] = "UserReliableSequenced9",
    [44] = "UserReliableSequenced10",
    [45] = "UserReliableSequenced11",
    [46] = "UserReliableSequenced12",
    [47] = "UserReliableSequenced13",
    [48] = "UserReliableSequenced14",
    [49] = "UserReliableSequenced15",
    [50] = "UserReliableSequenced16",
    [51] = "UserReliableSequenced17",
    [52] = "UserReliableSequenced18",
    [53] = "UserReliableSequenced19",
    [54] = "UserReliableSequenced20",
    [55] = "UserReliableSequenced21",
    [56] = "UserReliableSequenced22",
    [57] = "UserReliableSequenced23",
    [58] = "UserReliableSequenced24",
    [59] = "UserReliableSequenced25",
    [60] = "UserReliableSequenced26",
    [61] = "UserReliableSequenced27",
    [62] = "UserReliableSequenced28",
    [63] = "UserReliableSequenced29",
    [64] = "UserReliableSequenced30",
    [65] = "UserReliableSequenced31",
    [66] = "UserReliableSequenced32",
    [67] = "UserReliableOrdered1",
    [68] = "UserReliableOrdered2",
    [69] = "UserReliableOrdered3",
    [70] = "UserReliableOrdered4",
    [71] = "UserReliableOrdered5",
    [72] = "UserReliableOrdered6",
    [73] = "UserReliableOrdered7",
    [74] = "UserReliableOrdered8",
    [75] = "UserReliableOrdered9",
    [76] = "UserReliableOrdered10",
    [77] = "UserReliableOrdered11",
    [78] = "UserReliableOrdered12",
    [79] = "UserReliableOrdered13",
    [80] = "UserReliableOrdered14",
    [81] = "UserReliableOrdered15",
    [82] = "UserReliableOrdered16",
    [83] = "UserReliableOrdered17",
    [84] = "UserReliableOrdered18",
    [85] = "UserReliableOrdered19",
    [86] = "UserReliableOrdered20",
    [87] = "UserReliableOrdered21",
    [88] = "UserReliableOrdered22",
    [89] = "UserReliableOrdered23",
    [90] = "UserReliableOrdered24",
    [91] = "UserReliableOrdered25",
    [92] = "UserReliableOrdered26",
    [93] = "UserReliableOrdered27",
    [94] = "UserReliableOrdered28",
    [95] = "UserReliableOrdered29",
    [96] = "UserReliableOrdered30",
    [97] = "UserReliableOrdered31",
    [98] = "UserReliableOrdered32",
    [99] = "Unused1",
    [100] = "Unused2",
    [101] = "Unused3",
    [102] = "Unused4",
    [103] = "Unused5",
    [104] = "Unused6",
    [105] = "Unused7",
    [106] = "Unused8",
    [107] = "Unused9",
    [108] = "Unused10",
    [109] = "Unused11",
    [110] = "Unused12",
    [111] = "Unused13",
    [112] = "Unused14",
    [113] = "Unused15",
    [114] = "Unused16",
    [115] = "Unused17",
    [116] = "Unused18",
    [117] = "Unused19",
    [118] = "Unused20",
    [119] = "Unused21",
    [120] = "Unused22",
    [121] = "Unused23",
    [122] = "Unused24",
    [123] = "Unused25",
    [124] = "Unused26",
    [125] = "Unused27",
    [126] = "Unused28",
    [127] = "Unused29",
    [128] = "LibraryError",
    [129] = "Ping",
    [130] = "Pong",
    [131] = "Connect",
    [132] = "ConnectResponse",
    [133] = "ConnectionEstablished",
    [134] = "Acknowledge",
    [135] = "Disconnect",
    [136] = "Discovery",
    [137] = "DiscoveryResponse",
    [138] = "NatPunchMessage",
    [139] = "NatIntroduction",
    [142] = "NatIntroductionConfirmRequest",
    [143] = "NatIntroductionConfirmed",
    [140] = "ExpandMTURequest",
    [141] = "ExpandMTUSuccess"
}

LIDGREN_HEADER_BYTE_SIZE = 5

lidgren_proto = Proto("lidgren", "Lidgren.Network v3 Protocol")

-- Enum declarations in Lidgren's code use dec, so dec it is here.
lidgren_proto.fields.msgtype = ProtoField.uint8("lidgren.msgtype", "Message Type", base.DEC, lidgren_message_types)
lidgren_proto.fields.fragmented = ProtoField.bool("lidgren.fragmented", "Fragmented", 16, nil, 0x0001)
lidgren_proto.fields.sequence = ProtoField.uint16("lidgren.sequence", "Sequence", base.DEC, nil, 0xFFFE)
lidgren_proto.fields.payloadbits = ProtoField.uint16("lidgren.payloadbits", "Payload length (bits)", base.DEC)
lidgren_proto.fields.payload = ProtoField.bytes("lidgren.payload", "Payload")
lidgren_proto.fields.fragment_group = ProtoField.uint32("lidgren.fragment_group", "Fragment group")
lidgren_proto.fields.fragment_group_bits = ProtoField.uint32("lidgren.fragment_group_bits", "Fragment group total bits size")
lidgren_proto.fields.fragment_chunk_byte_size = ProtoField.uint32("lidgren.fragment_chunk_byte_size", "Fragment chunk byte size")
lidgren_proto.fields.fragment_chunk_number = ProtoField.uint32("lidgren.fragment_chunk_number", "Fragment chunk number")

-- Returns uint value, length of field
function lidgren_varuint(buffer)
    local len = 0
    local sum = 0
    local shift = 0

    while true do
        local byte = buffer(len, 1):uint()
        -- sum |= (byte & 0x7f) << (shift & 0x1f)
        sum = bit.bor(sum, bit.lshift(bit.band(byte, 0x7f), bit.band(shift, 0x1f)))
        shift = shift + 7
        len = len + 1
        if bit.band(byte, 0x80) == 0 then
            return sum, len
        end
    end
end

-- Returns: remaining buffer, nil if done.
function lidgren_parsemessage(buffer, tree, index, msgtypes)
    local message_type = buffer(0,1):uint()
    local message_type_name = lidgren_message_types[message_type]
    table.insert(msgtypes, message_type_name)
 
    tree:add_le(lidgren_proto.fields.msgtype, buffer(0, 1))
    tree:add_le(lidgren_proto.fields.fragmented, buffer(1, 2))
    tree:add_le(lidgren_proto.fields.sequence, buffer(1, 2))
    tree:add_le(lidgren_proto.fields.payloadbits, buffer(3, 2))

    local payloadbits = buffer(3, 2):le_uint()
    local payloadbytes = math.ceil(payloadbits / 8)

    local payload_offset = 5

    local fragmented = bit.band(buffer(1, 2):le_uint(), 1)

    if fragmented == 1 then
        -- Fragmentation header fields are variable-length.
        local frag_group, len = lidgren_varuint(buffer(payload_offset))
        tree:add_le(lidgren_proto.fields.fragment_group, buffer(payload_offset, len), frag_group)
        payload_offset = payload_offset + len

        local frag_group_total_bits, len = lidgren_varuint(buffer(payload_offset))
        tree:add_le(lidgren_proto.fields.fragment_group_bits, buffer(payload_offset, len), frag_group_total_bits)
        payload_offset = payload_offset + len

        local frag_chunk_byte_size, len = lidgren_varuint(buffer(payload_offset))
        tree:add_le(lidgren_proto.fields.fragment_chunk_byte_size, buffer(payload_offset, len), frag_chunk_byte_size)
        payload_offset = payload_offset + len

        local frag_chunk_number, len = lidgren_varuint(buffer(payload_offset))
        tree:add_le(lidgren_proto.fields.fragment_chunk_number, buffer(payload_offset, len), frag_chunk_number)
        payload_offset = payload_offset + len
    end

    tree:add_le(lidgren_proto.fields.payload, buffer(payload_offset, payloadbytes))

    tree:set_len(payload_offset + payloadbytes)
    local fragmented_str = "False"
    if fragmented == 1 then
        fragmented_str = "True"
    end
    tree:set_text(string.format(
        "Message #%d, Bits: %d, Type: %s (%d), Fragmented: %s",
        index,
        payloadbits,
        message_type_name,
        message_type,
        fragmented_str))

    -- You can't have 0-length TvbRanges for some ridiculous reason. Great.
    local nextoffset = payload_offset + payloadbytes
    if nextoffset == buffer:len() then
        return nil
    end

    return buffer(nextoffset)
end

function lidgren_proto.dissector(buffer, pinfo, tree)
    pinfo.cols.protocol = "LIDGREN"
    local subtree = tree:add(lidgren_proto,buffer(), "Lidgren.Network v3 Protocol Data")

    local msgtypes = {}
    local countmsg = 1;

    while buffer:len() > LIDGREN_HEADER_BYTE_SIZE do
        local msgtree = subtree:add(buffer(), "")
        local remaining = lidgren_parsemessage(buffer, msgtree, countmsg, msgtypes)
        if remaining == nil then 
            break
        end
        buffer = remaining
        countmsg = countmsg + 1
    end

    pinfo.columns.info:append(" " .. table.concat(msgtypes, ", "))
end

udp_table = DissectorTable.get("udp.port")
-- This is just the default port for Robust. Dunno if there's a better way to make it "idk pick the port yourself" for Wireshark.
udp_table:add(1212, lidgren_proto)

