extern "C"
{
    char* GetSystemLanguage()
	{
        const char* sysLangString = [[[NSLocale preferredLanguages] objectAtIndex:0] UTF8String];
        char* copy = (char*)malloc(strlen(sysLangString) + 1);
        strcpy(copy, sysLangString);
        return copy;
	}
}