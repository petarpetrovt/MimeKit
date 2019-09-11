﻿//
// TextConverterTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Text;

using MimeKit.Text;

using NUnit.Framework;

namespace UnitTests.Text {
	[TestFixture]
	public class TextConverterTests
	{
		void AssertArgumentExceptions (TextConverter converter)
		{
			Assert.Throws<ArgumentNullException> (() => converter.InputEncoding = null);
			Assert.Throws<ArgumentNullException> (() => converter.OutputEncoding = null);

			Assert.Throws<ArgumentOutOfRangeException> (() => converter.InputStreamBufferSize = -1);
			Assert.Throws<ArgumentOutOfRangeException> (() => converter.OutputStreamBufferSize = -1);

			Assert.Throws<ArgumentNullException> (() => converter.Convert (null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, Stream.Null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (Stream.Null, (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (Stream) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((Stream) null, new StreamWriter (Stream.Null)));
			Assert.Throws<ArgumentNullException> (() => converter.Convert (new StreamReader (Stream.Null), (TextWriter) null));
			Assert.Throws<ArgumentNullException> (() => converter.Convert ((TextReader) null, new StreamWriter (Stream.Null)));
		}

		[Test]
		public void TestArgumentExceptions ()
		{
			AssertArgumentExceptions (new TextToText ());
			AssertArgumentExceptions (new TextToFlowed ());
			AssertArgumentExceptions (new TextToHtml ());

			AssertArgumentExceptions (new HtmlToHtml ());
		}

		void AssertDefaultPropertyValues (TextConverter converter)
		{
			Assert.IsFalse (converter.DetectEncodingFromByteOrderMark, "DetectEncodingFromByteOrderMark");
			Assert.IsNull (converter.Footer, "Footer");
			Assert.IsNull (converter.Header, "Header");
			Assert.AreEqual (Encoding.UTF8, converter.InputEncoding, "InputEncoding");
			Assert.AreEqual (Encoding.UTF8, converter.OutputEncoding, "OutputEncoding");
			Assert.AreEqual (4096, converter.InputStreamBufferSize, "InputStreamBufferSize");
			Assert.AreEqual (4096, converter.OutputStreamBufferSize, "OutputStreamBufferSize");
		}

		[Test]
		public void TestDefaultPropertyValues ()
		{
			AssertDefaultPropertyValues (new TextToText ());
			AssertDefaultPropertyValues (new TextToFlowed ());
			AssertDefaultPropertyValues (new TextToHtml ());

			AssertDefaultPropertyValues (new HtmlToHtml ());
		}

		[Test]
		public void TestSimpleTextToText ()
		{
			string expected = "This is some sample text. This is line #1." + Environment.NewLine +
				"This is line #2." + Environment.NewLine +
				"And this is line #3." + Environment.NewLine;
			string text = "This is some sample text. This is line #1." + Environment.NewLine +
				"This is line #2." + Environment.NewLine +
				"And this is line #3." + Environment.NewLine;
			var converter = new TextToText { Header = null, Footer = null };
			var result = converter.Convert (text);

			Assert.AreEqual (TextFormat.Text, converter.InputFormat, "InputFormat");
			Assert.AreEqual (TextFormat.Text, converter.OutputFormat, "OutputFormat");
			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleTextToFlowed ()
		{
			string expected = "> Thou art a villainous ill-breeding spongy dizzy-eyed reeky elf-skinned " + Environment.NewLine +
				">  pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including " + Environment.NewLine +
				">>>>  the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			string text = "> Thou art a villainous ill-breeding spongy dizzy-eyed reeky elf-skinned pigeon-egg!" + Environment.NewLine +
				">> Thou artless swag-bellied milk-livered dismal-dreaming idle-headed scut!" + Environment.NewLine +
				">>> Thou errant folly-fallen spleeny reeling-ripe unmuzzled ratsbane!" + Environment.NewLine +
				">>>> Henceforth, the coding style is to be strictly enforced, including the use of only upper case." + Environment.NewLine +
				">>>>> I've noticed a lack of adherence to the coding styles, of late." + Environment.NewLine +
				">>>>>> Any complaints?" + Environment.NewLine;
			TextConverter converter = new TextToFlowed { Header = null, Footer = null };
			string result = converter.Convert (text);

			Assert.AreEqual (TextFormat.Text, converter.InputFormat, "InputFormat");
			Assert.AreEqual (TextFormat.Flowed, converter.OutputFormat, "OutputFormat");
			Assert.AreEqual (expected, result);

			converter = new FlowedToText { DeleteSpace = true };
			result = converter.Convert (expected);

			Assert.AreEqual (TextFormat.Flowed, converter.InputFormat, "InputFormat");
			Assert.AreEqual (TextFormat.Text, converter.OutputFormat, "OutputFormat");
			Assert.AreEqual (text, result);
		}

		[Test]
		public void TestSimpleTextToHtml ()
		{
			const string expected = "This is some sample text. This is line #1.<br/>" +
				"This is line #2.<br/>" +
				"And this is line #3.<br/>";
			string text = "This is some sample text. This is line #1." + Environment.NewLine +
				"This is line #2." + Environment.NewLine +
				"And this is line #3." + Environment.NewLine;
			var converter = new TextToHtml { Header = null, Footer = null, OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.AreEqual (TextFormat.Text, converter.InputFormat, "InputFormat");
			Assert.AreEqual (TextFormat.Html, converter.OutputFormat, "OutputFormat");
			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestSimpleTextWithUrlsToHtml ()
		{
			const string expected = "Check out <a href=\"http://www.xamarin.com\">http://www.xamarin.com</a> - it&#39;s amazing!<br/>";
			string text = "Check out http://www.xamarin.com - it's amazing!" + Environment.NewLine;
			var converter = new TextToHtml { Header = null, Footer = null, OutputHtmlFragment = true };
			var result = converter.Convert (text);

			Assert.AreEqual (TextFormat.Text, converter.InputFormat, "InputFormat");
			Assert.AreEqual (TextFormat.Html, converter.OutputFormat, "OutputFormat");
			Assert.AreEqual (expected, result);
		}

		void ReplaceUrlsWithFileNames (HtmlTagContext ctx, HtmlWriter htmlWriter)
		{
			if (ctx.TagId == HtmlTagId.Image) {
				htmlWriter.WriteEmptyElementTag (ctx.TagName);
				ctx.DeleteEndTag = true;

				for (int i = 0; i < ctx.Attributes.Count; i++) {
					var attr = ctx.Attributes[i];

					if (attr.Id == HtmlAttributeId.Src) {
						var fileName = Path.GetFileName (attr.Value);
						htmlWriter.WriteAttributeName (attr.Name);
						htmlWriter.WriteAttributeValue (fileName);
					} else {
						htmlWriter.WriteAttribute (attr);
					}
				}
			} else {
				ctx.WriteTag (htmlWriter, true);
			}
		}

		[Test]
		public void TestSimpleHtmlToHtml ()
		{
			string expected = File.ReadAllText ("../../TestData/html/xamarin3.xhtml");
			string text = File.ReadAllText ("../../TestData/html/xamarin3.html");
			var converter = new HtmlToHtml { Header = null, Footer = null, HtmlTagCallback = ReplaceUrlsWithFileNames };
			var result = converter.Convert (text);

			Assert.AreEqual (TextFormat.Html, converter.InputFormat, "InputFormat");
			Assert.AreEqual (TextFormat.Html, converter.OutputFormat, "OutputFormat");
			Assert.AreEqual (expected, result);
		}
	}
}
