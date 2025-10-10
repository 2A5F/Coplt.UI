cmake_minimum_required(VERSION 3.16)

if (WIN32)
    set(CMAKE_C_COMPILER "clang-cl.exe")
    set(CMAKE_CXX_COMPILER "clang-cl.exe")
    set(VCPKG_TARGET_TRIPLET "x64-windows-static" CACHE STRING "Vcpkg triplet")
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
elseif (APPLE)
    set(VCPKG_TARGET_TRIPLET "x64-osx-static" CACHE STRING "Vcpkg triplet")
elseif (UNIX)
    set(VCPKG_TARGET_TRIPLET "x64-linux-static" CACHE STRING "Vcpkg triplet")
endif()

project(Coplt.Ui)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

add_definitions(-march=skylake)
add_definitions(-Wno-everything)

add_subdirectory(ThirdParty/emhash)

add_subdirectory(Coplt.Com2)
add_subdirectory(Coplt.Ui.Native)
