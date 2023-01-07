cmake_minimum_required(VERSION 3.0)

project(VMTranslator)
add_executable(VMTranslator main.cpp Parser.cpp Writer.cpp Commands.cpp)

set_property(TARGET VMTranslator PROPERTY CXX_STANDARD 14)
install(TARGETS VMTranslator RUNTIME DESTINATION bin)
target_link_libraries(${PROJECT_NAME} stdc++fs)
