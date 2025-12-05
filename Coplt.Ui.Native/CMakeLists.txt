project(Coplt.Ui.Native LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)

include(CheckIPOSupported)
check_ipo_supported(RESULT lto_supported OUTPUT lto_error)

add_definitions(-DUNICODE)
add_definitions(-D_UNICODE)
add_definitions(-DCPPTRACE_STATIC_DEFINE)
add_definitions(-DNOMINMAX)

find_package(cpptrace CONFIG REQUIRED)
find_package(mimalloc CONFIG REQUIRED)
find_package(fmt CONFIG REQUIRED)

add_library(${PROJECT_NAME} SHARED src/Build.cc src/Compute.cc src/dwrite/Compute.cc)
target_compile_definitions(${PROJECT_NAME} PRIVATE -D COPLT_SOURCE)
set_property(TARGET ${PROJECT_NAME} PROPERTY
        MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
if (lto_supported)
    message(STATUS "IPO / LTO enabled")
    set_property(TARGET ${PROJECT_NAME} PROPERTY INTERPROCEDURAL_OPTIMIZATION TRUE)
else ()
    message(STATUS "IPO / LTO not supported: <${lto_error}>")
endif ()
target_link_libraries(${PROJECT_NAME} PRIVATE
        Coplt::Com
        coplt_ui_rusty
        cpptrace::cpptrace
        mimalloc-static
        fmt::fmt-header-only
)
if (WIN32)
    target_link_libraries(${PROJECT_NAME} PRIVATE icuuc.lib)
endif ()
