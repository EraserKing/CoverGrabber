CoverGrabber
============

Grab cover image, track name/artists/year and lyric from Xiami and write to local files.

Since I bought an iPod Classic, I found it very uncomfortable if there's any album without cover :(
So I wrote this tool to help OCDers, which could fill cover image (and something more) for your local files.

## Features ##
* Grab cover image
* Grab album title / album artist / track name / track artist / year
* Grab lyric
* Write to local file (By taglib#)
* Support Xiami only (with verify code)

## 3rd Party Components ##
* HtmlAgilityPack 1.4.6 (http://htmlagilitypack.codeplex.com/)
Microsoft Public License (Ms-PL)

* TagLib# 2.1.0 (https://www.nuget.org/packages/taglib/)
GNU Lesser General Public License

## Known issues ##
* Single thread - UI hangs up during processing
* Error if file in use

## Todo ##
* Error Handling
* Multi-thread
* Support 163 Cloud Music
