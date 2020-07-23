local function dump(value, desciption, nesting)
    if type(nesting) ~= "number" then nesting = 3 end
 
    local lookupTable = {}
    local result = {}
 
    local function _v(v)
        if type(v) == "string" then
            v = "\"" .. v .. "\""
        end
        return tostring(v)
    end
 
 
    local function _dump(value, desciption, indent, nest, keylen)
        desciption = desciption or "<var>"
        local spc = ""
        if type(keylen) == "number" then
            spc = string.rep(" ", keylen - string.len(_v(desciption)))
        end
        if type(value) ~= "table" then
            result[#result +1 ] = string.format("%s%s%s = %s", indent, _v(desciption), spc, _v(value))
        elseif lookupTable[value] then
            result[#result +1 ] = string.format("%s%s%s = *REF*", indent, desciption, spc)
        else
            lookupTable[value] = true
            if nest > nesting then
                result[#result +1 ] = string.format("%s%s = *MAX NESTING*", indent, desciption)
            else
                result[#result +1 ] = string.format("%s%s = {", indent, _v(desciption))
                local indent2 = indent.."    "
                local keys = {}
                local keylen = 0
                local values = {}
                for k, v in pairs(value) do
                    keys[#keys + 1] = k
                    local vk = _v(k)
                    local vkl = string.len(vk)
                    if vkl > keylen then keylen = vkl end
                    values[k] = v
                end
                table.sort(keys, function(a, b)
                    if type(a) == "number" and type(b) == "number" then
                        return a < b
                    else
                        return tostring(a) < tostring(b)
                    end
                end)
                for i, k in ipairs(keys) do
                    _dump(values[k], k, indent2, nest + 1, keylen)
                end
                result[#result +1] = string.format("%s}", indent)
            end
        end
    end
    _dump(value, desciption, "- ", 1)
 
    for i, line in ipairs(result) do
        print(line)
    end
end


local Node = {}
Node.__index = Node

function Node.new( o )
    local self = o or {
        name = "",
        -- 之所以区分开，是因为作为父节点的系统，也有可能会被增加红点，这样一来当前节点的总数量就不能单靠子节点累加得出了
        apointNum = 0,--子节点加上自己的红点数量
        cpointNum = 0,--子节点的红点数量
        spointNum = 0,--自己的红点数量
        parent = nil,
        cbf = nil,

        child = {}
    }
    setmetatable(self, Node)

    return self
end

function Node:SetCallBack( cbf )
    self.cbf = cbf
end

function Node:SetPointNum( num )
    self.spointNum = num
    self.apointNum = self.spointNum + self.cpointNum
    self:NotifyChange()

    self:UpdateParent()
end

function Node:UpdateParent(  )
    if self.parent then
        self.parent:ChangePointNum()
    end
end

function Node:NotifyChange(  )
    if self.cbf then
        self.cbf(self)
    else
        print("好像没有设置代理函数: ", self.name)
    end
end

function Node:ChangePointNum()
    local cpNum = 0
    if self.child and next(self.child) then
        for k,v in pairs(self.child) do
            cpNum = cpNum + v.apointNum
        end
    end
    self.cpointNum = cpNum
    self.apointNum = self.spointNum + self.cpointNum
    self:NotifyChange()

    self:UpdateParent()
end

local function split(str, p, nilIfEmpty)
    local insert = table.insert
    local fields = {}
    local pattern = string.format("[^%s]+", p)
    for w in str:gmatch(pattern) do insert(fields, w) end

    if p == "." then p = "%." end
    if (str:find(p)) == 1 then
        table.insert(fields, 1, "")
    end

    if nilIfEmpty and #fields == 0 then return nil end
    return fields
end

local P = {}
P.__index = P

P.SysList = {

    Main = "Main",
    Mail = "Main.Mail",
    SysMail = "Main.Mail.SystemMail",
    TeamMail = "Main.Mail.TeamMail",
    GuardMail = "Main.Mail.Guard"

}

function P.new(  )
    local self = {
        root = nil
    }
    setmetatable(self, P)
    self:init()
    return self
end

function P:init()
    local list
    self.root = Node.new()
    self.root.name = P.SysList.Main

    local node
    for k,v in pairs(P.SysList) do
        node = self.root
        list = split(v, ".")
        if list[1] ~= self.root.name then
            print("事件不是从main开始的")

        else
            if #list > 1 then
                for i=2,#list do
                    if not node.child[list[i]] then
                        node.child[list[i]] = Node.new()
                    end
                    node.child[list[i]].name = list[i]
                    node.child[list[i]].parent = node
                    node.child[list[i]].apointNum = 0
                    node.child[list[i]].cpointNum = 0
                    node.child[list[i]].spointNum = 0
                    node.child[list[i]].child = node.child[list[i]].child or {}

                    node = node.child[list[i]]
                end
            end
        end
    end

end

function P:GetNode( name )
    local list = split(name, ".")
    if #list == 1 then
        return self.root
    end
    local node = self.root
    if #list > 1 then
        for i=2,#list do
            -- print(list[i], dump(node))
            node = node.child[list[i]]
        end
    end
    
    return node
end

function P:SetCallBack(name, cbf)
    local node = self:GetNode(name)
    if node then
        node:SetCallBack(cbf)
    end
end

function P:UpdatePoint( name, num )
    local node = self:GetNode(name)
    if node then
        node:SetPointNum(num)
    end
end

local ss = P.new()
local n = ss:GetNode(P.SysList.Mail)

ss:SetCallBack(P.SysList.Mail, function (node)
    print("this is "..P.SysList.Mail..", current num is "..node.apointNum)
    -- dump(node)
end)
ss:SetCallBack(P.SysList.SysMail, function (node)
    print("this is "..P.SysList.SysMail..", current num is "..node.apointNum)
    -- dump(node)
end)
ss:SetCallBack(P.SysList.TeamMail, function (node)
    print("this is "..P.SysList.TeamMail..", current num is "..node.apointNum)
    -- dump(node)
end)
ss:SetCallBack(P.SysList.GuardMail, function (node)
    print("this is "..P.SysList.GuardMail..", current num is "..node.apointNum)
    -- dump(node)
end)

ss:UpdatePoint(P.SysList.GuardMail, 1)
ss:UpdatePoint(P.SysList.TeamMail, 1)
ss:UpdatePoint(P.SysList.SysMail, 2)
ss:UpdatePoint(P.SysList.Mail, 2)
-- ss:UpdatePoint(P.SysList.Mail, 0)
-- ss:UpdatePoint(P.SysList.SysMail, 0)
-- ss:UpdatePoint(P.SysList.GuardMail, 0)
-- ss:UpdatePoint(P.SysList.TeamMail, 0)

-- for k,v in pairs(n.child) do
--     print(k,v)
-- end

