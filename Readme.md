# Higurashi BGM Name Matcher

## Overview

This tool is used to match Higurashi BGM files based on their audio similarity (or by an exact match).

I'm uploading this here even though I doubt this will be used often because:

- We might need to use it on the next Higurashi chapter
- It might be useful if we ever need to match audio files automatically by content

Unfortunately, the current implementation has some problems, but it works for the most part. You should consider re-writing it for your application, or using a different audio matching library which better suits your application (see known problems below).

I'm not 100% certain that you will be able to run this program on your system without issue (for example because I forgot to upload some files or you need some third party thing installed), so let me know if you have issues running the program.

## Known Problems

### Matching library doesn't support short files (less than 1-2 seconds)

The matching library's default settins cause very short files to never match (and to also have problems with deserialization if short files are attempted to be added). For this reason, short files are never matched by audio similarity, and are only matched by MD5/exact match.

I think it is theoretically possible to configure the library for short files, but I tried and couldn't get it working.

### Manual matching by MD5 does not use actual original file's MD5 values

I added matching files by their MD5 checksum, where the program will consider two files matching if they have the same MD5. But I also transcoded all the files to low quality .opus to reduce file size so the archive could be more easily uploaded/downloaded.

This means that if you copy a file from one of our mods, the program won't detect it because it will have a different MD5 checksum to my encoded .opus files.

What I really should have done is recorded the MD5 values in a .csv file, rather than having the files on disk (in the manual_matching folder). Then you could assign multiple MD5 values to a name, and also not need to store the files on disk.

### Matching is not deterministic

The matching library's default query method uses randomness to avoid having two files which are slightly offset in time not matching. This randomness doesn't seem to be deterministic, or possibly my program is introducing some non-determinisim somewhere along the way.

This causes matches to not always be the same, even if you run the program twice without changing anything.

## Resultant BGM filename to BGM name mapping

The BGM name mapping is stored in the `mod_usable_files` folder. There is one JSON file for each corresponding folder in each of our game types (question, answer, rei, and console). Then there is one variant for each folder in the StreamingAssets folder (such as BGM, OGBGM, and ExtraBGM)

Note that all the question arcs have the same BGM files, and all the answer arcs have the same BGM files, so there is not a separate.

## Usage

1. Clone this repo
2. Download [this archive containing bgm, and pre-generated matching databases](https://1drv.ms/u/s!Ar-lAVeetlqhhL9a7ftu_Si8_wnDsA?e=pyaZ6k), and extract it to the root of the repository
3. If you are on a Unix-like OS, make sure that FFMPeg 4.x.x is installed. I have not tested Unix, you may have problems running on Unix.
4. Add any additional files you want to match to the `mod` folder.
5. Run the program with `dotnet run`. You'll need .net 6 or higher installed.
6. If you add new files to the folders in `reference/FOLDER_NAME`, to regenerate the database, delete the folder called `db_FOLDER_NAME`
7. To update the stored list of names, copy all the .json and .csv files into the `mod_usable_files` folder

## TODO

- Add OST Remake BGM to matching
