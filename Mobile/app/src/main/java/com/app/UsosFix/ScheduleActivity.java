package com.app.UsosFix;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.DisplayMetrics;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonArrayRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.material.tabs.TabLayout;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.ParseException;
import java.util.Locale;

public class ScheduleActivity extends BaseActivity {

    private String scheduleBase;
    private JSONArray scheduleResponse;
    private JSONArray USOSScheduleResponse;
    private JSONArray exchangesScheduleResponse;
    private String currentDay;
    private Activity context;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        context = this;

        GetUserInformation(new VolleyCallback() {
            @Override
            public void onSuccess() throws JSONException {
                SetLanguage();

                setContentView(R.layout.activity_schedule);

                if (savedInstanceState == null) {
                    Bundle extras = getIntent().getExtras();
                    if(extras == null) {
                        scheduleBase = null;
                    } else {
                        scheduleBase = extras.getString("scheduleBase");
                    }
                } else {
                    scheduleBase = (String) savedInstanceState.getSerializable("scheduleBase");
                }

                currentDay = "Monday";
                final TabLayout weekTabLayout = findViewById(R.id.weekTabLayout);
                weekTabLayout.addOnTabSelectedListener(new TabLayout.OnTabSelectedListener() {
                    @Override
                    public void onTabSelected(TabLayout.Tab tab) {
                        try {
                            switch (tab.getPosition()) {
                                case 0:
                                    currentDay = "Monday";
                                    break;
                                case 1:
                                    currentDay = "Tuesday";
                                    break;
                                case 2:
                                    currentDay = "Wednesday";
                                    break;
                                case 3:
                                    currentDay = "Thursday";
                                    break;
                                case 4:
                                    currentDay = "Friday";
                                    break;
                                case 5:
                                    currentDay = "Saturday";
                                    break;
                                case 6:
                                    currentDay = "Sunday";
                                    break;
                            }
                            AddClassesCustomViews(currentDay);
                        } catch (JSONException | ParseException e) {
                            e.printStackTrace();
                        }
                    }

                    @Override
                    public void onTabUnselected(TabLayout.Tab tab) {
                        ViewGroup scheduleFrameLayout = findViewById(R.id.scheduleFrameLayout);
                        LinearLayout scheduleHorizontalLinesLayout = findViewById(R.id.scheduleHorizontalLinesLayout);
                        scheduleFrameLayout.removeAllViews();
                        scheduleFrameLayout.addView(scheduleHorizontalLinesLayout);
                    }

                    @Override
                    public void onTabReselected(TabLayout.Tab tab) { }
                });

                GetScheduleInformation(new VolleyCallback() {
                    @Override
                    public void onSuccess() throws JSONException {
                        Button USOSBasedButton = findViewById(R.id.usosBasedButton);

                        if (scheduleBase == null || scheduleBase.equals("USOS")) {
                            ChooseUSOSBasedSchedule(USOSBasedButton);
                        }
                    }
                });
                GetAfterExchangesScheduleInformation(new VolleyCallback() { //TUTAJ MI OSTATNIO RZUCAŁO TIMEOUTEM
                    @Override
                    public void onSuccess() throws JSONException {
                        Button exchangesBasedButton = findViewById(R.id.exchangesBasedButton);

                        if (scheduleBase != null && scheduleBase.equals("exchanges")) {
                            ChooseUSOSBasedSchedule(exchangesBasedButton);
                        }
                    }
                });
            }
        });
    }
    public void ChooseUSOSBasedSchedule(View view) {
        ViewGroup scheduleFrameLayout = findViewById(R.id.scheduleFrameLayout);
        LinearLayout scheduleHorizontalLinesLayout = findViewById(R.id.scheduleHorizontalLinesLayout);
        scheduleFrameLayout.removeAllViews();
        scheduleFrameLayout.addView(scheduleHorizontalLinesLayout);

        Button USOSBasedButton = (Button)view;
        Button exchangesBasedButton = findViewById(R.id.exchangesBasedButton);

        scheduleBase = "USOS";
        scheduleResponse = USOSScheduleResponse;

        USOSBasedButton.setBackgroundResource(R.drawable.button_background);
        USOSBasedButton.setTextColor(getResources().getColor(R.color.colorWhite));
        USOSBasedButton.setEnabled(false);

        exchangesBasedButton.setBackgroundResource(R.drawable.outlined_button_background);
        exchangesBasedButton.setTextColor(getResources().getColor(R.color.colorPrimary));
        exchangesBasedButton.setEnabled(true);

        try {
            AddClassesCustomViews(currentDay);
        } catch (JSONException | ParseException e) {
            ShowPopup(context, context.findViewById(R.id.scheduleActivity), getString(R.string.schedule_display_error), true);
        }
    }
    public void ChooseExchangesBasedSchedule(View view) {
        ViewGroup scheduleFrameLayout = findViewById(R.id.scheduleFrameLayout);
        LinearLayout scheduleHorizontalLinesLayout = findViewById(R.id.scheduleHorizontalLinesLayout);
        scheduleFrameLayout.removeAllViews();
        scheduleFrameLayout.addView(scheduleHorizontalLinesLayout);

        Button USOSBasedButton = findViewById(R.id.usosBasedButton);
        Button exchangesBasedButton = (Button)view;

        scheduleBase = "exchanges";
        scheduleResponse = exchangesScheduleResponse;

        exchangesBasedButton.setBackgroundResource(R.drawable.button_background);
        exchangesBasedButton.setTextColor(getResources().getColor(R.color.colorWhite));
        exchangesBasedButton.setEnabled(false);

        USOSBasedButton.setBackgroundResource(R.drawable.outlined_button_background);
        USOSBasedButton.setTextColor(getResources().getColor(R.color.colorPrimary));
        USOSBasedButton.setEnabled(true);

        try {
            AddClassesCustomViews(currentDay);
        } catch (JSONException | ParseException e) {
            ShowPopup(context, context.findViewById(R.id.scheduleActivity), getString(R.string.schedule_display_error), true);
        }
    }

    private void GetScheduleInformation(final VolleyCallback callback) {

        if (AuthorizedToken == null) return;

        String url = getResources().getString(R.string.base_url) + "/Timetable/UserGroups?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        USOSScheduleResponse = response;
                        try {
                            callback.onSuccess();
                        } catch (JSONException e) {
                            ShowPopup(context, context.findViewById(R.id.scheduleActivity), getString(R.string.schedule_display_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.scheduleActivity), getString(R.string.schedule_error), true);
            }
        });

        // Add the request to the RequestQueue.
        queue.add(jsonArrayRequest);
    }
    private void GetAfterExchangesScheduleInformation(final VolleyCallback callback) {

        if (AuthorizedToken == null) return;

        String url = getResources().getString(R.string.base_url) + "/Timetable/UserGroupsAfterExchanges?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        exchangesScheduleResponse = response;
                        try {
                            callback.onSuccess();
                        } catch (JSONException e) {
                            ShowPopup(context, context.findViewById(R.id.scheduleActivity), getString(R.string.schedule_display_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.scheduleActivity), getString(R.string.schedule_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }

    private void AddClassesCustomViews(String dayOfTheWeek) throws JSONException, ParseException {
        String currentLang = getResources().getConfiguration().locale.getLanguage();
        ViewGroup scheduleFrameLayout = findViewById(R.id.scheduleFrameLayout);

        for (int i = 0; i < scheduleResponse.length(); i++) {
            JSONObject classJSONinfo = scheduleResponse.getJSONObject(i);
            String startTime = classJSONinfo.getJSONArray("meetings").getJSONObject(0).getString("startTime");
            if (getDayStringOld(startTime, Locale.ENGLISH).equals(dayOfTheWeek)) {
                String subjectName = classJSONinfo.getJSONObject("subject").getJSONObject("name").getString(currentLang);
                String room = classJSONinfo.getJSONArray("meetings").getJSONObject(0).getString("room");
                String rawType = classJSONinfo.getString("classType");
                startTime = startTime.split("T")[1].replaceFirst("^0+(?!$)", "");
                startTime = startTime.substring(0, startTime.lastIndexOf(":"));
                String endTime = classJSONinfo.getJSONArray("meetings").getJSONObject(0).getString("endTime").split("T")[1].replaceFirst("^0+(?!$)", "");
                endTime = endTime.substring(0, endTime.lastIndexOf(":"));
                String hours = startTime + " - " + endTime;
                final String subjectId = classJSONinfo.getJSONObject("subject").getString("id");

                FrameLayout newClassElement = SetClassElement(rawType, hours, subjectName, room, Integer.parseInt(subjectId), false);

                if (classJSONinfo.getString("state").equals("Submitted"))
                    newClassElement = SetSubmittedClassElement(newClassElement, rawType);
                else if (classJSONinfo.getString("state").equals("Rejected")) {
                    newClassElement.setBackground(getResources().getDrawable(R.drawable.rejected_class_background));
                    newClassElement.setForeground(getResources().getDrawable(R.drawable.gray_class_strip));
                }
                newClassElement.setOnClickListener(new View.OnClickListener() {
                     @Override
                     public void onClick(View v) {
                        Intent subjectScheduleIntent = new Intent(getApplicationContext(), SubjectScheduleActivity.class);
                        subjectScheduleIntent.putExtra("AuthorizedToken", AuthorizedToken);
                        subjectScheduleIntent.putExtra("SubjectId", subjectId);
                        startActivity(subjectScheduleIntent);
                     }
                 });

                FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MATCH_PARENT, CountHeightFromDuration(startTime, endTime));
                params.setMargins(2, CountPosition(startTime), 2, 0);
                scheduleFrameLayout.addView(newClassElement, params);
            }
        }
    }
    @SuppressLint("UseCompatLoadingForDrawables")
    private FrameLayout SetClassElement(String classType, String hours, String subject, String location, int id, boolean exchanged) {
        FrameLayout classElement = (FrameLayout) getLayoutInflater().inflate(R.layout.new_class_element, null);
        switch (classType) {
            case "CWI":
                classElement.setBackground(getResources().getDrawable(R.drawable.tutorial_background));
                break;
            case "LAB":
                classElement.setBackground(getResources().getDrawable(R.drawable.laboratory_background));
                break;
            case "PRO":
                classElement.setBackground(getResources().getDrawable(R.drawable.project_background));
                break;
        }

        TextView hoursTextView = classElement.findViewById(R.id.classElement_hoursTextView);
        hoursTextView.setText(hours);

        TextView typeTextView = classElement.findViewById(R.id.classElement_typeTextView);
        typeTextView.setText(TranslateClassType(classType));

        TextView subjectTextView = classElement.findViewById(R.id.classElement_subjectTextView);
        subjectTextView.setText(subject);

        TextView placeTextView = classElement.findViewById(R.id.classElement_placeTextView);
        placeTextView.setText(location);

        return classElement;
    }
    private FrameLayout SetSubmittedClassElement(FrameLayout classElement, String classType) {
        classElement.setBackground(getResources().getDrawable(R.drawable.submitted_lecture_background));
        switch (classType) {
            case "CWI":
                classElement.setBackground(getResources().getDrawable(R.drawable.submitted_tutorial_background));
                break;
            case "LAB":
                classElement.setBackground(getResources().getDrawable(R.drawable.submitted_lab_background));
                break;
            case "PRO":
                classElement.setBackground(getResources().getDrawable(R.drawable.submitted_project_background));
                break;
        }

        return classElement;
    }

    private String TranslateClassType(String classType) {
        switch (classType) {
            case "WYK":
                return getString(R.string.WYK);
            case "CWI":
                return getString(R.string.CWI);
            case "LAB":
                return getString(R.string.LAB);
            case "PRO":
                return getString(R.string.PRO);
            default:
                return "";
        }
    }

    private int CountHeightFromDuration(String startTime, String endTime) {
        int startHour = Integer.parseInt(startTime.substring(0, startTime.indexOf(":")));
        int startMinutes = Integer.parseInt(startTime.substring(startTime.indexOf(":") + 1));
        int endHour = Integer.parseInt(endTime.substring(0, endTime.indexOf(":")));
        int endMinutes = Integer.parseInt(endTime.substring(endTime.indexOf(":") + 1));

        return ConvertToDp(64 * (endHour - startHour) + 16 * (endMinutes - startMinutes) / 15);
    }
    private int CountPosition(String startTime) {
        int startHour = Integer.parseInt(startTime.substring(0, startTime.indexOf(":")));
        int startMinutes = Integer.parseInt(startTime.substring(startTime.indexOf(":") + 1));

        return ConvertToDp(64 * (startHour - 7) - 1 + 16 * startMinutes / 15); //zaczynamy od godziny 7 rano
    }
    private int ConvertToDp(int value) {
        DisplayMetrics metrics = getApplicationContext().getResources().getDisplayMetrics();
        float duration = (float)(value);
        return (int)(metrics.density * duration + 0.5f);
    }
}