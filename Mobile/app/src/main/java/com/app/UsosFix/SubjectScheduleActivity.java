package com.app.UsosFix;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonArrayRequest;
import com.android.volley.toolbox.Volley;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.ParseException;
import java.util.HashMap;
import java.util.Map;

public class SubjectScheduleActivity extends BaseActivity {

    private String SubjectId;
    private Activity context;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        context = this;

        if (savedInstanceState == null) {
            Bundle extras = getIntent().getExtras();
            if(extras == null) {
                SubjectId = "-1";
            } else {
                SubjectId = extras.getString("SubjectId");
            }
        } else {
            SubjectId = (String)savedInstanceState.getSerializable("SubjectId");
        }

        setContentView(R.layout.activity_subject_schedule);
        GetSubjectInformation();
    }
    private void GetSubjectInformation() {
        if (SubjectId == "-1") {
            ShowPopup(context, context.findViewById(R.id.subjectScheduleActivity), getString(R.string.subject_id_error), true);
            return;
        }
        String url = getResources().getString(R.string.base_url) + "/Timetable/SubjectGroups?token=" + AuthorizedToken + "&subjectId=" + SubjectId;
        // Instantiate the RequestQueue.
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        try {
                            DisplayInformation(response);
                        } catch (JSONException | ParseException e) {
                            ShowPopup(context, context.findViewById(R.id.subjectScheduleActivity), getString(R.string.subject_groups_display_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.subjectScheduleActivity), getString(R.string.subject_groups_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }
    private void DisplayInformation(JSONArray subjectScheduleArray) throws JSONException, ParseException {
        String currentLang = getResources().getConfiguration().locale.getLanguage();

        TextView subjectName = findViewById(R.id.subjectSchedule_subjectNameTextView);
        subjectName.setText(subjectScheduleArray.getJSONObject(0).getJSONObject("subject").getJSONObject("name").getString(currentLang));

        LinearLayout lecturesList = findViewById(R.id.lecturesList);
        LinearLayout tutorialsList = findViewById(R.id.tutorialsList);
        LinearLayout laboratoriesList = findViewById(R.id.laboratoriesList);
        LinearLayout projectsList = findViewById(R.id.projectsList);

        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
        LayoutInflater inflater = (LayoutInflater)getApplicationContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        params.setMargins(0, 5, 0, 5);

        for (int i = 0; i < subjectScheduleArray.length(); i++) {
            final JSONObject group = subjectScheduleArray.getJSONObject(i);

            View groupCard = inflater.inflate(R.layout.group_card, lecturesList, false);
            switch (group.getString("classType")) {
                case "WYK":
                    lecturesList.addView(groupCard, params);
                    break;
                case "CWI":
                    tutorialsList.addView(groupCard, params);
                    break;
                case "LAB":
                    laboratoriesList.addView(groupCard, params);
                    break;
                case "PRO":
                    projectsList.addView(groupCard, params);
                    break;
            }

            TextView groupNumber = groupCard.findViewById(R.id.groupNumberTextView);
            groupNumber.setText(getString(R.string.group).concat(" nr ").concat(group.getString("groupNumber")));

            TextView lecturer = groupCard.findViewById(R.id.lecturerTextView);
            lecturer.setText(group.getString("lecturers"));

            TextView day = groupCard.findViewById(R.id.dayTextView);
            String date = ConvertDates(group.getJSONArray("meetings"));
            day.setText(date.substring(0, date.indexOf(" ")));

            TextView hours = groupCard.findViewById(R.id.hoursTextView);
            hours.setText(date.substring(date.indexOf(" ") + 1, date.length()));

            TextView place = groupCard.findViewById(R.id.placeTextView);
            place.setText(group.getJSONArray("meetings").getJSONObject(0).getString("room"));

            groupCard.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    Intent UserGroupIntent = new Intent(v.getContext(), UserGroupActivity.class);
                    UserGroupIntent.putExtra("AuthorizedToken", AuthorizedToken);

                    String groupId = "";
                    try {
                        groupId = group.getString("id");
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }

                    UserGroupIntent.putExtra("GroupId", groupId);
                    startActivity(UserGroupIntent);
                }
            });
        }

        if (lecturesList.getChildCount() > 0)
            lecturesList.getChildAt(lecturesList.getChildCount() - 1).setBackground(getResources().getDrawable(R.drawable.white_element_content_background));
        if (tutorialsList.getChildCount() > 0)
            tutorialsList.getChildAt(tutorialsList.getChildCount() - 1).setBackground(getResources().getDrawable(R.drawable.white_element_content_background));
        if (laboratoriesList.getChildCount() > 0)
            laboratoriesList.getChildAt(laboratoriesList.getChildCount() - 1).setBackground(getResources().getDrawable(R.drawable.white_element_content_background));
        if (projectsList.getChildCount() > 0)
            projectsList.getChildAt(projectsList.getChildCount() - 1).setBackground(getResources().getDrawable(R.drawable.white_element_content_background));
    }

    private String ConvertDates(JSONArray meetings) throws JSONException, ParseException {
        Map<String, Integer> map = new HashMap<String, Integer>();
        int maxCount = 0;
        String mostFrequentDate = "";

        for (int i = 0; i < meetings.length(); i++) {
            String day = meetings.getJSONObject(i).getString("startTime");
            String dayOfTheWeek = getDayStringOld(day, getResources().getConfiguration().locale);
            String startTime = day.split("T")[1].replaceFirst("^0+(?!$)", "");
            startTime = startTime.substring(0, startTime.lastIndexOf(":"));
            String endTime = meetings.getJSONObject(i).getString("endTime").split("T")[1].replaceFirst("^0+(?!$)", "");
            endTime = endTime.substring(0, endTime.lastIndexOf(":"));
            String resultDay = dayOfTheWeek.concat(" ").concat(startTime).concat("-").concat(endTime);
            int count = map.containsKey(resultDay) ? map.get(resultDay) : 0;
            map.put(resultDay, count + 1);
            if (map.get(resultDay) > maxCount) mostFrequentDate = resultDay;
        }

        return mostFrequentDate;
    }
}