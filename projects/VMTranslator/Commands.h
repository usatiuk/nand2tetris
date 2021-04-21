#ifndef COMMANDS_H
#define COMMANDS_H

#include <iostream>
#include <string>
#include <unordered_map>

enum class CommandType {
    Add,
    Sub,
    Neg,
    Eq,
    Gt,
    Lt,
    And,
    Or,
    Not,
    Push,
    Pop,
    Label,
    IfGoto,
    Goto,
    If,
    Function,
    Call,
    Return,
    All,
    Empty
};

extern std::unordered_map<std::string, CommandType> CommandTypeDict;
extern std::unordered_map<CommandType, std::string> TypeCommandDict;

struct Command {
    CommandType type = CommandType::Empty;
    std::string arg1 = "";
    int arg2 = 0;

    Command() {}
    Command(CommandType type) : type(type) {}
    Command(CommandType type, std::string arg1, int arg2 = 0)
        : type(type), arg1(arg1), arg2(arg2) {}

    inline bool isEmpty() { return type == CommandType::Empty; }

    friend std::ostream &operator<<(std::ostream &os, const Command &cmd);
};

#endif  // COMMANDS_H
