#include "Commands.h"

std::unordered_map<std::string, CommandType> CommandTypeDict = {
    {"", CommandType::Empty},
    {"push", CommandType::Push},
    {"pop", CommandType::Pop},
    {"add", CommandType::Add},
    {"sub", CommandType::Sub},
    {"neg", CommandType::Neg},
    {"eq", CommandType::Eq},
    {"gt", CommandType::Gt},
    {"lt", CommandType::Lt},
    {"and", CommandType::And},
    {"or", CommandType::Or},
    {"not", CommandType::Not},
    {"label", CommandType::Label},
    {"if-goto", CommandType::IfGoto},
    {"goto", CommandType::Goto},
    {"call", CommandType::Call},
    {"function", CommandType::Function},
    {"return", CommandType::Return},
};

std::unordered_map<CommandType, std::string> TypeCommandDict = []() {
    std::unordered_map<CommandType, std::string> rev;
    for (auto &cmd : CommandTypeDict) {
        rev[cmd.second] = cmd.first;
    }
    return rev;
}();

std::ostream &operator<<(std::ostream &os, const Command &cmd) {
    os << TypeCommandDict[cmd.type] << " " << cmd.arg1 << " " << cmd.arg2;
    return os;
}
