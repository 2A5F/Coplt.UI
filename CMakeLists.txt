cmake_minimum_required(VERSION 3.16)

if (WIN32)
    set(CMAKE_C_COMPILER "clang-cl.exe")
    set(CMAKE_CXX_COMPILER "clang-cl.exe")
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif ()

project(Coplt.Ui)

include(ExternalProject)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

add_definitions(-march=skylake)
add_definitions(-Wno-everything)

add_subdirectory(ThirdParty/emhash)
add_subdirectory(ThirdParty/corrosion)

add_subdirectory(Coplt.Com2)
add_subdirectory(Coplt.Ui.Native)

corrosion_import_crate(MANIFEST_PATH Coplt.Ui.Native/rust_part/Cargo.toml)
