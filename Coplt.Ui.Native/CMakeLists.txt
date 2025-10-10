project(Coplt.Ui.Native LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 23)

add_definitions(-DUNICODE)
add_definitions(-D_UNICODE)
add_definitions(-DCPPTRACE_STATIC_DEFINE)
add_definitions(-DNOMINMAX)

find_package(cpptrace CONFIG REQUIRED)
find_package(harfbuzz CONFIG REQUIRED)
find_package(mimalloc CONFIG REQUIRED)
find_package(fmt CONFIG REQUIRED)

if (WIN32)
    set(BACKEND_SOURCES
            src/dwrite/Backend.cc
            src/dwrite/Backend.h
            src/dwrite/SystemFontCollection.cc
            src/dwrite/SystemFontCollection.h
            src/dwrite/FontFamily.cc
            src/dwrite/FontFamily.h
            src/dwrite/Error.h
            src/dwrite/Font.cc
            src/dwrite/Font.h
            src/FontFace.cc
            src/FontFace.h
            src/dwrite/FontFace.cc
            src/dwrite/FontFace.h
    )
endif ()

add_library(${PROJECT_NAME} SHARED
        src/lib.cc
        src/lib.h
        src/Error.cc
        src/Error.h
        src/Backend.h
        ${BACKEND_SOURCES}
)
set_property(TARGET ${PROJECT_NAME} PROPERTY
        MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
target_link_libraries(${PROJECT_NAME} PRIVATE
        Coplt::Com
        cpptrace::cpptrace
        mimalloc-static
        fmt::fmt-header-only
        harfbuzz::harfbuzz
)
if (WIN32)
    target_link_libraries(${PROJECT_NAME} PRIVATE Dwrite.lib)
endif ()
