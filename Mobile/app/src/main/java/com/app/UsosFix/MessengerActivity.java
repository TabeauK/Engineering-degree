package com.app.UsosFix;

import android.app.Activity;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;

import com.android.volley.NetworkResponse;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.HttpHeaderParser;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class MessengerActivity extends BaseActivity {
    private HubConnection hubConnection;
    private String ConversationId;
    private String myId;
    private LinearLayout messagesLinearLayout;
    private Activity context;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_messenger);
        context = this;

        messagesLinearLayout = findViewById(R.id.messagesLinearLayout);

        SharedPreferences sharedPreferences = getSharedPreferences("userId", Context.MODE_PRIVATE);
        myId = sharedPreferences.getString("userId", "-1");

        if (savedInstanceState == null) {
            Bundle extras = getIntent().getExtras();
            if(extras == null) {
                ConversationId = null;
            } else {
                ConversationId = extras.getString("ConversationId");
            }
        } else {
            ConversationId = (String) savedInstanceState.getSerializable("ConversationId");
        }

        String url = getResources().getString(R.string.base_url) + "/chat?token=".concat(AuthorizedToken);
        hubConnection = HubConnectionBuilder.create(url).build();

        if (hubConnection.getConnectionState() == HubConnectionState.DISCONNECTED)
            hubConnection.start();

        hubConnection.on("Receive", (message) -> {
            JsonObject messageObject = new Gson().toJsonTree(message).getAsJsonObject();
            context.runOnUiThread(() -> DisplayIncomingMessage(messageObject));
        }, Object.class);

        GetMessages();
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED)
            hubConnection.stop();
        super.onOptionsItemSelected(item);
        return true;
    }

    @Override
    public void onBackPressed() {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED)
            hubConnection.stop();

        Intent ConversationsIntent = new Intent(context, ConversationsActivity.class);
        ConversationsIntent.putExtra("AuthorizedToken", AuthorizedToken);
        startActivity(ConversationsIntent);
    }

    private void DisplayIncomingMessage(JsonObject message) {
        if (Double.parseDouble(message.get("conversationId").toString()) != Double.parseDouble(ConversationId))
            return;
        if (Double.parseDouble(myId) == Double.parseDouble(message.get("authorId").toString()))
            return;
        LinearLayout messageLayout = (LinearLayout) getLayoutInflater().inflate(R.layout.their_message, null);
        TextView contentTextView = messageLayout.findViewById(R.id.messageText);
        TextView timeTextView = messageLayout.findViewById(R.id.messageTime);
        String[] dateAndTime = message.get("sentAt").toString().substring(1, 17).split("T"); //[date, time]
        String content = message.getAsJsonObject("content").get("pl").toString();
        content = content.substring(1, content.length() - 1);
        timeTextView.setText(dateAndTime[1]);
        contentTextView.setText(content);
        messagesLinearLayout.addView(messageLayout);

        ScrollView scrollView = findViewById(R.id.messagesScrollView);
        scrollView.post(new Runnable() {
            @Override
            public void run() {
                scrollView.fullScroll(View.FOCUS_DOWN);
            }
        });
    }

    private void GetMessages() {
        String url = getResources().getString(R.string.base_url) + "/Messages/MessagesSince?token=" + AuthorizedToken + "&conversationId=" + ConversationId;
        RequestQueue queue = Volley.newRequestQueue(this);

        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        try {
                            CreateParticipantsAndMessagesLists(response);
                        } catch (JSONException e) {
                            ShowPopup(context, context.findViewById(R.id.messengerActivityLayout), getString(R.string.messages_display_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.messengerActivityLayout), getString(R.string.messages_error), true);

            }
        });

        queue.add(jsonObjectRequest);
    }
    private void CreateParticipantsAndMessagesLists(JSONObject conversation) throws JSONException {
        JSONArray messages = conversation.getJSONArray("messages");

        for (int i = 0; i < messages.length(); i++) {
            String authorId = messages.getJSONObject(i).getString("authorId");
            LinearLayout message;
            TextView date, time, content;

            if (authorId.equals(myId)) {
                message = (LinearLayout) getLayoutInflater().inflate(R.layout.my_message, null);
            }
            else {
                message = (LinearLayout) getLayoutInflater().inflate(R.layout.their_message, null);
            }
            date = message.findViewById(R.id.messageDate);
            content = message.findViewById(R.id.messageText);
            time = message.findViewById(R.id.messageTime);
            String[] dateAndTime = messages.getJSONObject(i).getString("sentAt").split("T");
            dateAndTime[1] = dateAndTime[1].substring(0, dateAndTime[1].lastIndexOf(":"));
            date.setText(dateAndTime[0]);
            time.setText(dateAndTime[1]);

            if (i < messages.length() - 1) {
                String prevDate = messages.getJSONObject(i + 1).getString("sentAt");
                prevDate = prevDate.substring(0, prevDate.indexOf("T"));
                if (dateAndTime[0].equals(prevDate))
                    message.removeViewAt(0);
            }

            content.setText(messages.getJSONObject(i).getJSONObject("content").getString(getResources().getConfiguration().locale.getLanguage()));
            messagesLinearLayout.addView(message, 0);
        }
        LinearLayout invitationWindow = findViewById(R.id.invitationWindow);
        if (messages.length() == 1 && !messages.getJSONObject(0).getString("authorId").equals(myId)) { //type.equals("Invite")) //zaproszenie do czatu, które trzeba zaakceptować (lub odrzucić)
            invitationWindow.setVisibility(View.VISIBLE);

            Button acceptanceButton = invitationWindow.findViewById(R.id.acceptButton);
            acceptanceButton.setOnClickListener(this::AcceptChat);

            Button rejectButton = invitationWindow.findViewById(R.id.rejectButton);
            rejectButton.setOnClickListener(this::RejectChat);
        }
        else if (messages.length() == 1) {
            LinearLayout sendMessageSection = findViewById(R.id.editTextAndButtonToSendMessage);
            sendMessageSection.setVisibility(View.INVISIBLE);
            invitationWindow.setVisibility(View.INVISIBLE);
        }
        else
            invitationWindow.setVisibility(View.INVISIBLE);

        ScrollView scrollView = findViewById(R.id.messagesScrollView);
        scrollView.post(new Runnable() {
            @Override
            public void run() {
                scrollView.fullScroll(View.FOCUS_DOWN);
            }
        });
    }

    public void SendMessage(View view) {
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            EditText messageEditText = findViewById(R.id.messageEditText);
            String message = messageEditText.getText().toString();
            if (message.length() < 1)
                ShowPopup(context, context.findViewById(R.id.messengerActivityLayout), getString(R.string.no_message_content_error), true);

            hubConnection.send("Send", message, Integer.parseInt(ConversationId));

            LinearLayout messagesLinearLayout = findViewById(R.id.messagesLinearLayout);
            LinearLayout messageLayout = (LinearLayout) getLayoutInflater().inflate(R.layout.my_message, null);
            TextView content = messageLayout.findViewById(R.id.messageText);
            content.setText(messageEditText.getText());
            messagesLinearLayout.addView(messageLayout);
            messageEditText.getText().clear();

            ScrollView scrollView = findViewById(R.id.messagesScrollView);
            scrollView.post(new Runnable() {
                @Override
                public void run() {
                    scrollView.fullScroll(View.FOCUS_DOWN);
                }
            });
        }
    }
    public void AcceptChat(View v) {
        if (AuthorizedToken == null || ConversationId == null) return;

        AcceptOrRejectInvitation("AcceptChat");
    }
    public void RejectChat(View v) {
        if (AuthorizedToken == null || ConversationId == null) return;

        AcceptOrRejectInvitation("RejectChat");
    }
    private void RestartActivity() {
        Intent messengerIntent = new Intent(getApplicationContext(), MessengerActivity.class);
        messengerIntent.putExtra("AuthorizedToken", AuthorizedToken);
        messengerIntent.putExtra("ConversationId", ConversationId);
        startActivity(messengerIntent);
    }
    private void AcceptOrRejectInvitation(String epType) {
        String url = getResources().getString(R.string.base_url) + "/Messages/" + epType + "?token=" + AuthorizedToken + "&conversationId=" + ConversationId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.PUT, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        try {
                            // response jest prawdopodobnie nullem
                            if (response != null)
                                myId = response.getString("id");
                            RestartActivity();
                        } catch (JSONException e) {
                            if (epType.equals("AcceptChat"))
                                ShowPopup(context, context.findViewById(R.id.messengerActivityLayout), getString(R.string.accept_chat_error), true);
                            else
                                ShowPopup(context, context.findViewById(R.id.messengerActivityLayout), getString(R.string.reject_chat_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                if (epType.equals("AcceptChat"))
                    ShowPopup(context, context.findViewById(R.id.messengerActivityLayout), getString(R.string.accept_chat_error), true);
                else
                    ShowPopup(context, context.findViewById(R.id.messengerActivityLayout), getString(R.string.reject_chat_error), true);
            }
        }) {
            @Override
            protected Response<JSONObject> parseNetworkResponse(NetworkResponse response) {
                return Response.success(null, HttpHeaderParser.parseCacheHeaders(response));
            }
        };

        queue.add(jsonObjectRequest);
    }

    public void MessageClicked(View view) {
        TextView messageTime = view.findViewById(R.id.messageTime);
        if (messageTime.getVisibility() == View.INVISIBLE)
            messageTime.setVisibility(View.VISIBLE);
        else
            messageTime.setVisibility(View.INVISIBLE);
    }
}