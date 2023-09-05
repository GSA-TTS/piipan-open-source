# Testing with Social Security Numbers

As much as possible, we avoid adding to the codebase Social Security Numbers that are potentially in-use. However, testing may require using valid SSN's.

GSA Shared Services has provided [The Human Capital Golden Test Set](https://ussm.gsa.gov/golden-data-test-set/) which contains valid SSN's, but it's unclear whether these SSN's are still in use.

The [test_ssns.csv](./test_ssns.csv) file is a list of SSN's that will [pass system validation](./pprl.md#social-security-number-ssn) but are no longer in use. They were taken from the [Death Master File](https://sortedbybirthdate.com/small_pages/1928/19280103_1000.html) from persons who died prior to the year 2000. The Social Security Administration does not reuse an SSN after a person dies. Although imperfect, the numbers in this file are the most suitable for testing that we've found to date. If you find a more suitable test data set, please [open an issue](https://github.com/18F/piipan/issues).

