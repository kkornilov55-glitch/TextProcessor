using System;
using Xunit;
using TextProcessor.Logic;

namespace TextProcessingTests
{

    public class NormalizationTests
    {
        private readonly Normalization _normalizer;

        public NormalizationTests()
        {
            _normalizer = new Normalization();
        }

        [Fact]
        public void Normalize_BasicText_ReturnsLowercaseWithoutPunctuation()
        {
            string input = "Привет, МИР!!!";
            string result = _normalizer.Normalize(input);
            Assert.Equal("привет мир", result);
        }

        [Fact]
        public void Normalize_TextWithSlashes_RemovesSlashes()
        {
            string input = "ПарковКаа на// улице запРещена";
            string result = _normalizer.Normalize(input);
            Assert.Equal("парковкаа на улице запрещена", result);
        }

        [Fact]
        public void Normalize_MultipleSpaces_CollapsesToSingleSpace()
        {
            string input = "привет    мир";
            string result = _normalizer.Normalize(input);
            Assert.Equal("привет мир", result);
        }

        [Fact]
        public void Normalize_LeadingTrailingSpaces_Trims()
        {
            string input = "   привет мир   ";
            string result = _normalizer.Normalize(input);
            Assert.Equal("привет мир", result);
        }

        [Fact]
        public void Normalize_MixedPunctuation_RemovesAll()
        {
            string input = "Привет! Как дела? Всё хорошо.";
            string result = _normalizer.Normalize(input);
            Assert.Equal("привет как дела всё хорошо", result);
        }

        [Fact]
        public void Normalize_NewlinesAndTabs_ReplacedWithSpaces()
        {
            string input = "привет\nмир\tкак\tдела";
            string result = _normalizer.Normalize(input);
            Assert.Equal("привет мир как дела", result);
        }

        [Fact]
        public void Normalize_NumbersPreserved_KeepsDigits()
        {
            string input = "Привет 2026 год!";
            string result = _normalizer.Normalize(input);
            Assert.Equal("привет 2026 год", result);
        }

        [Fact]
        public void Normalize_SingleWord_ReturnsLowercase()
        {
            string input = "ПРИВЕТ";
            string result = _normalizer.Normalize(input);
            Assert.Equal("привет", result);
        }

        [Fact]
        public void Normalize_NullText_ThrowsArgumentNullException()
        {
            string input = null;
            Assert.Throws<ArgumentNullException>(() => _normalizer.Normalize(input));
        }

        [Fact]
        public void Normalize_ResultContainsNoUppercase()
        {
            string input = "ПРИВЕТ Мир";
            string result = _normalizer.Normalize(input);
            Assert.Equal(result.ToLower(), result);
        }

        [Fact]
        public void Normalize_ResultContainsNoDoubleSpaces()
        {
            string input = "привет    мир";
            string result = _normalizer.Normalize(input);
            Assert.DoesNotContain("  ", result);
        }

        [Fact]
        public void Normalize_ResultContainsNoPunctuation()
        {
            string input = "Привет, мир! Как дела?";
            string result = _normalizer.Normalize(input);
            Assert.Matches(@"^[\w\s]*$", result);
        }

        [Fact]
        public void Normalize_ResultIsNotNull()
        {
            string input = "Привет";
            string result = _normalizer.Normalize(input);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("ПРИВЕТ", "привет")]
        [InlineData("привет", "привет")]
        [InlineData("ПрИвЕт", "привет")]
        public void Normalize_VariousCases_ReturnsLowercase(string input, string expected)
        {
            string result = _normalizer.Normalize(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("привет,мир", "приветмир")]
        [InlineData("привет!мир", "приветмир")]
        [InlineData("привет.мир", "приветмир")]
        [InlineData("привет//мир", "приветмир")]
        public void Normalize_VariousPunctuation_RemovesAll(string input, string expected)
        {
            string result = _normalizer.Normalize(input);
            Assert.Equal(expected, result);
        }
    }
}