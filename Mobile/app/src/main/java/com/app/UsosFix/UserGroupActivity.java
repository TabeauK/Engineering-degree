package com.app.UsosFix;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.widget.Button;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;

import androidx.core.content.res.ResourcesCompat;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonArrayRequest;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.Locale;

public class UserGroupActivity extends BaseActivity {

    private String GroupId;
    private boolean Joining; // Does user want to join this group?
    private ArrayList<String> invitedUsers;
    private JSONObject GroupInfo;
    private String SubjectId;
    private Activity context;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Joining = false;
        context = this;

        if (savedInstanceState == null) {
            Bundle extras = getIntent().getExtras();
            if(extras == null) {
                GroupId = "-1";
            } else {
                GroupId = extras.getString("GroupId");
            }
        } else {
            GroupId = (String)savedInstanceState.getSerializable("GroupId");
        }
        invitedUsers = new ArrayList<String>();

        setContentView(R.layout.activity_user_group);
        GetGroupInformation(new VolleyCallback() {
            @Override
            public void onSuccess() {
                GetExchanges();
                GetTeams();
            }
        });
    }
    @Override
    protected void GetTeams_onResponseFun(JSONObject response) throws JSONException, ParseException {
        JSONArray teams = response.getJSONArray("partOf");
        for (int i = 0; i < teams.length(); i++) //iterujemy po zespołach
        {
            if (teams.getJSONObject(i).getJSONObject("subject").getString("id").equals(SubjectId))
            {
                JSONArray invited = teams.getJSONObject(i).getJSONArray("users");
                for (int j = 0; j < invited.length(); j++)
                    invitedUsers.add(invited.getJSONObject(j).getString("id"));
                invited = teams.getJSONObject(i).getJSONArray("invitations");
                for (int j = 0; j < invited.length(); j++)
                    invitedUsers.add(invited.getJSONObject(j).getJSONObject("invited").getString("id"));
            }
        }
        JSONArray invitations = response.getJSONArray("invitedTo");
        for (int i = 0; i < invitations.length(); i++) //iterujemy po zaproszeniach do zespołów
        {
            if (invitations.getJSONObject(i).getJSONObject("subject").getString("id").equals(SubjectId))
            {
                JSONArray invited = invitations.getJSONObject(i).getJSONArray("users");
                for (int j = 0; j < invited.length(); j++)
                    invitedUsers.add(invited.getJSONObject(j).getString("id"));
                invited = invitations.getJSONObject(i).getJSONArray("invitations");
                for (int j = 0; j < invited.length(); j++)
                    invitedUsers.add(invited.getJSONObject(j).getJSONObject("invited").getString("id"));
            }
        }
        SetGroupInformation();
    }
    private void GetGroupInformation(final VolleyCallback callback) {
        if (GroupId == "-1") {
            ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.group_id_error), true);
            return;
        }
        String url = getResources().getString(R.string.base_url) + "/Timetable/GroupInfo?token=" + AuthorizedToken + "&groupId=" + GroupId;
        // Instantiate the RequestQueue.
        RequestQueue queue = Volley.newRequestQueue(this);

        // Request a string response from the provided URL.
        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        GroupInfo = response;
                        try {
                            SubjectId = response.getJSONObject("subject").getString("id");
                            callback.onSuccess();
                        } catch (JSONException e) {
                            ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.group_info_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.group_info_error), true);
            }
        });

        // Add the request to the RequestQueue.
        queue.add(jsonObjectRequest);
    }
    private void GetExchanges() {
        String url = getResources().getString(R.string.base_url) + "/Exchanges/Exchanges?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        int i = 0;
                        Joining = false;
                        try {
                            while (i < response.length() && !GroupId.equals(response.getJSONObject(i).getJSONObject("groupTo").getString("id"))) {
                                i++;
                            }
                            if (i < response.length())
                                Joining = true;
                        } catch (JSONException e) {
                                e.printStackTrace();
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.userGroupLinearLayout), getString(R.string.exchanges_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }
    // display
    private void SetGroupInformation() throws JSONException, ParseException {
        String currentLang = getResources().getConfiguration().locale.getLanguage();

        TextView subjectNameTextView = findViewById(R.id.subjectNameTextView);
        TextView classesTypeAndNumberTextView = findViewById(R.id.classesTypeAndNumberTextView);
        TextView studentsLimitTextView = findViewById(R.id.studentsLimitTextView);
        TextView studentsNumberTextView = findViewById(R.id.studentsNumberTextView);

        subjectNameTextView.setText(GroupInfo.getJSONObject("subject").getJSONObject("name").getString(currentLang));
        String classType = GetClassTypeName(GroupInfo.getString("classType")).concat(", ").concat(getString(R.string.group_nr)).concat(" ").concat(GroupInfo.getString("groupNumber"));
        classesTypeAndNumberTextView.setText(classType);

        TextView instructorTextView = findViewById(R.id.instructorTextView);
        instructorTextView.setText(GroupInfo.getString("lecturers"));

        TextView placeTextView = findViewById(R.id.placeTextView);
        placeTextView.setText(GroupInfo.getJSONArray("meetings").getJSONObject(0).getString("room"));

        studentsLimitTextView.setText(GroupInfo.getString("maxMembers"));
        studentsNumberTextView.setText(GroupInfo.getString("currentMembers"));

        TableLayout datesTable = findViewById(R.id.datesTableLayout);

        ArrayList<ArrayList<String>> meetings = SortMeetingDates(GroupInfo.getJSONArray("meetings"));
        for (int i = 0; i < meetings.size(); i++) {
            String[] start = meetings.get(i).get(0).split("T");
            String[] end = meetings.get(i).get(1).split("T");

            TableRow row = new TableRow(this);
            row.setPadding(0, 10, 0, 10);

            TextView number = CreateTextViewForDataTable();
            TextView dayOfAWeek = CreateTextViewForDataTable();
            TextView date = CreateTextViewForDataTable();
            TextView startHour = CreateTextViewForDataTable();
            TextView endHour = CreateTextViewForDataTable();

            number.setText(String.valueOf(i+1));
            number.setGravity(Gravity.CENTER_HORIZONTAL);
            row.addView(number);

            dayOfAWeek.setText(getDayStringOld(start[0], getResources().getConfiguration().locale));
            row.addView(dayOfAWeek);

            date.setText(start[0]);
            row.addView(date);

            start[1] = start[1].replaceFirst("^0+(?!$)", "");
            end[1] = end[1].replaceFirst("^0+(?!$)", "");

            startHour.setText(start[1].substring(0, start[1].lastIndexOf(":")));
            row.addView(startHour);

            endHour.setText(end[1].substring(0, end[1].lastIndexOf(":")));
            row.addView(endHour);

            datesTable.addView(row);
        }

        // IF STUDENT ALREADY WANTS TO JOIN THE GROUP
        Button joinButton = findViewById(R.id.joinGroupButton);
        Button stopJoiningButton = findViewById(R.id.stopJoiningGroupButton);
        joinButton.setVisibility(View.VISIBLE);
        if (Joining) {
            joinButton.setVisibility(View.INVISIBLE);
            joinButton.setEnabled(false);
            stopJoiningButton.setVisibility(View.VISIBLE);
            stopJoiningButton.setEnabled(true);
        }

        TableLayout studentsTable = findViewById(R.id.participantsTableLayout);
        final JSONArray jsonStudentsArray = GroupInfo.getJSONArray("students");
        for (int i = 0; i < jsonStudentsArray.length(); i++) {
            final JSONObject studentObject = jsonStudentsArray.getJSONObject(i);

            TableRow row = new TableRow(this);
            row.setPadding(0, 10, 0, 10);

            TextView number = CreateTextViewForStudentsTable();
            number.setText(String.valueOf(i + 1));
            number.setPadding(30, 0, 30, 0);
            row.addView(number);

            TextView studentTextView = CreateTextViewForStudentsTable();
            studentTextView.setText(studentObject.getString("displayName"));
            studentTextView.setTextAlignment(View.TEXT_ALIGNMENT_TEXT_START);
            studentTextView.setPadding(50, 0, 0, 0);
            row.addView(studentTextView);

            Button button = new Button(this);
            button.setBackground(getResources().getDrawable(R.drawable.button_background));
            button.setText(getString(R.string.invite));
            button.setTextSize(15);
            button.setTextColor(getResources().getColor(R.color.colorWhite));
            button.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_bold));
            button.setAllCaps(false);
            button.setMinHeight(0);
            button.setMinimumHeight(0);
            button.setPadding(0, 0, 0, 0);
            button.setHeight((int) convertDpToPx(this, 40f));
            button.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    // Invite student to your team
                    try {
                        int studentId = Integer.parseInt(studentObject.getString("id"));
                        int subjectId = Integer.parseInt(GroupInfo.getJSONObject("subject").getString("id"));
                        SendInvitation(studentId, subjectId);
                        button.setEnabled(false);
                        button.setText(R.string.invited);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                }
            });

            SharedPreferences sharedPreferences = getSharedPreferences("userId", Context.MODE_PRIVATE);
            String id = sharedPreferences.getString("userId", "-1");

            if (invitedUsers.contains(studentObject.getString("id"))) {
                button.setEnabled(false);
                button.setText(R.string.invited);
            }
            if (studentObject.getString("id").equals(id)) {
                joinButton.setVisibility(View.INVISIBLE);
                joinButton.setEnabled(false);
                stopJoiningButton.setVisibility(View.INVISIBLE);
                stopJoiningButton.setEnabled(false);
                button.setVisibility(View.INVISIBLE);
                button.setEnabled(false);
            }

            row.addView(button);
            studentsTable.addView(row);
        }
    }
    private ArrayList<ArrayList<String>> SortMeetingDates(JSONArray meetings) throws JSONException {
        ArrayList<ArrayList<String>> dates = new ArrayList<>();
        for (int i = 0; i < meetings.length(); i++) {
            JSONObject meeting = meetings.getJSONObject(i);
            String start = meeting.getString("startTime");
            String end = meeting.getString("endTime");

            if (dates.size() == 0)
                dates.add(new ArrayList<>(Arrays.asList(start, end)));
            else {
                int j = 0;
                while (j < i && j < dates.size() && start.compareTo(dates.get(j).get(0)) > 0) {
                    j++;
                }
                dates.add(j, new ArrayList<>(Arrays.asList(start, end)));
            }
        }
        return dates;
    }
    private TextView CreateTextViewForDataTable() {
        TextView textView = new TextView(this);
        textView.setTextSize(15);
        textView.setGravity(Gravity.LEFT);
        textView.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_light));
        textView.setTextColor(getResources().getColor(R.color.colorBlack));
        return textView;
    }
    private TextView CreateTextViewForStudentsTable() {
        TextView textView = new TextView(this);
        textView.setTextSize(15);
        textView.setGravity(Gravity.CENTER_HORIZONTAL);
        textView.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_semi_bold));
        textView.setTextColor(getResources().getColor(R.color.colorBlack));
        return textView;
    }
    @Override
    public String getDayStringOld(String stringDate, Locale locale) throws ParseException {
        SimpleDateFormat formatter1 = new SimpleDateFormat("yyyy-MM-dd");
        Date date = formatter1.parse(stringDate);
        DateFormat formatter2 = new SimpleDateFormat("EEEE", locale);
        return formatter2.format(date);
    }
    private String GetClassTypeName(String classType) {
        switch (classType) {
            case "WYK":
                return getApplicationContext().getString(R.string.WYK);
            case "CWI":
                return getApplicationContext().getString(R.string.CWI);
            case "LAB":
                return getApplicationContext().getString(R.string.LAB);
            case "PRO":
                return getApplicationContext().getString(R.string.PRO);
        }
        return "";
    }
    private void SendInvitation(int invitedId, int subjectId) {
        String url = getResources().getString(R.string.base_url) + "/Teams/InviteUser?token=" + AuthorizedToken + "&invitedId=" + String.valueOf(invitedId) + "&subjectId=" + String.valueOf(subjectId);
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.inv_to_team_success), false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.inv_to_team_error), true);
            }
        });

        queue.add(stringRequest);
    }
    public void JoinToGroupRequest(View view) {
        String url = getResources().getString(R.string.base_url) + "/Exchanges/AddExchange?token=" + AuthorizedToken + "&groupToId=" + GroupId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.add_exchange_success), false);
                        Button joinButton = findViewById(R.id.joinGroupButton);
                        Button stopJoiningButton = findViewById(R.id.stopJoiningGroupButton);
                        joinButton.setVisibility(View.INVISIBLE);
                        joinButton.setEnabled(false);
                        stopJoiningButton.setVisibility(View.VISIBLE);
                        stopJoiningButton.setEnabled(true);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.add_exchange_error), true);
            }
        });

        queue.add(stringRequest);
    }

    public void StopJoiningToGroupRequest(View view) {
        String url = getResources().getString(R.string.base_url) + "/Exchanges/DeleteExchangeByGroupId?token=" + AuthorizedToken + "&groupId=" + GroupId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.DELETE, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.remove_exchange_success), false);
                        Button joinButton = findViewById(R.id.joinGroupButton);
                        Button stopJoiningButton = findViewById(R.id.stopJoiningGroupButton);
                        joinButton.setVisibility(View.VISIBLE);
                        joinButton.setEnabled(true);
                        stopJoiningButton.setVisibility(View.INVISIBLE);
                        stopJoiningButton.setEnabled(false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userGroupLinearLayout), getString(R.string.remove_exchange_error), true);
            }
        });

        queue.add(stringRequest);
    }
}