project(Coplt.Ui.TextLayout.Native LANGUAGES CXX)

add_definitions(-DUNICODE)
add_definitions(-D_UNICODE)
add_definitions(-DCPPTRACE_STATIC_DEFINE)
add_definitions(-DNOMINMAX)

find_package(cpptrace CONFIG REQUIRED)
find_package(harfbuzz CONFIG REQUIRED)
find_package(mimalloc CONFIG REQUIRED)

add_library(${PROJECT_NAME} SHARED
        src/lib.cc
        src/lib.h
        src/Face.cc
        src/Face.h
)
set_property(TARGET ${PROJECT_NAME} PROPERTY
        MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
target_link_libraries(${PROJECT_NAME} PRIVATE
        Coplt::Com
        cpptrace::cpptrace
        harfbuzz::harfbuzz
        mimalloc-static
)
