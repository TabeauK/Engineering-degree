package com.app.UsosFix;

import org.junit.Test;

import java.text.ParseException;
import java.util.Locale;

import static org.junit.Assert.assertEquals;

/**
 * Example local unit test, which will execute on the development machine (host).
 *
 * @see <a href="http://d.android.com/tools/testing">Testing documentation</a>
 */

public class ExampleUnitTest {

    @Test
    public void addition_isCorrect() {
        assertEquals(4, 2 + 2);
    }

    @Test
    public void GetDayOfTheWeekTest() throws ParseException {
        String stringDate = "2020-12-07T08:00:00";
        Locale loc = new Locale("en");
        String resultDay = ScheduleActivity.getDayStringOld(stringDate, loc);
        assertEquals("Monday", resultDay);
    }
}