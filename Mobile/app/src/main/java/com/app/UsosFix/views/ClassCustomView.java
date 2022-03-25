package com.app.UsosFix.views;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.view.View;

import androidx.annotation.Nullable;

public class ClassCustomView extends View {

    private Rect coloredRect;
    private Rect whiteRect;
    private Paint paintColoredRect;
    private Paint paintWhiteRect;

    private Rect darkerColoredRect;
    private Paint paintDarkerColoredRect;

    private String classDuration;
    private String classType;
    private String classSubject;
    private String classLocation;
    private int classId; //groupId
    private boolean afterExchange;

    private Paint paintText;

    public ClassCustomView(Context context) {
        super(context);
        init(null);
    }

    public ClassCustomView(Context context, @Nullable AttributeSet attrs) {
        super(context, attrs);
        init(attrs);
    }

    public ClassCustomView(Context context, @Nullable AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init(attrs);
    }

    private void init(@Nullable AttributeSet set) {
        coloredRect = new Rect();
        whiteRect = new Rect();
        paintColoredRect = new Paint();
        paintWhiteRect = new Paint();
        paintText = new Paint();

        afterExchange = false;
        darkerColoredRect = new Rect();
        paintDarkerColoredRect = new Paint();
    }

    public void setText(String hours, String type, String subject, String location, int id, boolean exchanged) {
        classDuration = hours;
        classType = type;
        classSubject = subject;
        classLocation = location;
        classId = id;
        afterExchange = exchanged;
    }

    @Override
    protected void onDraw(Canvas canvas) {

        coloredRect.set(0, 0, getWidth(), getHeight());
        whiteRect.set(2, getHeight() / 2, getWidth() - 2, getHeight() - 2);

        setColoredRectColor();
        canvas.drawRect(coloredRect, paintColoredRect);

        if (afterExchange) { //potencjalna grupa po wymianie
            DrawStrips(canvas);
        }

        paintWhiteRect.setColor(Color.WHITE);
        canvas.drawRect(whiteRect, paintWhiteRect);

        paintText.setColor(Color.WHITE);
        paintText.setTextSize(40f);
        canvas.drawText(classDuration, 20, 50, paintText);

        paintText.setTextAlign(Paint.Align.LEFT);
        canvas.drawText(classType, getWidth() - 100, 50, paintText);

        canvas.drawText(classSubject, 20, 100, paintText);

        paintText.setColor(Color.BLACK);
        canvas.drawText(classLocation, 20, getHeight() / 2 + 40, paintText);
    }

    private void DrawStrips(Canvas canvas) {
        for (int left = 0; left < getWidth(); left += 30) {
            darkerColoredRect.set(left, 0, Math.min(left + 15, getWidth()), getHeight());
            canvas.drawRect(darkerColoredRect, paintDarkerColoredRect);
        }
    }

    private void setColoredRectColor() {
        switch (classType) {
            case "WYK":
            case "LEC":
                paintColoredRect.setColor(Color.parseColor("#273562")); //granatowy
                paintDarkerColoredRect.setColor(Color.parseColor("#18213d"));
                break;
            case "TUT":
            case "CWI":
                paintColoredRect.setColor(Color.parseColor("#644885")); //fioletowy
                paintDarkerColoredRect.setColor(Color.parseColor("#412f57"));
                break;
            case "LAB":
                paintColoredRect.setColor(Color.parseColor("#bc3369")); //malinowy
                paintDarkerColoredRect.setColor(Color.parseColor("#87264c"));
                break;
            case "PRO":
                paintColoredRect.setColor(Color.parseColor("#136d75")); //ciemny morski
                paintDarkerColoredRect.setColor(Color.parseColor("#0d4c52"));
                break;
            default:
                paintColoredRect.setColor(Color.WHITE);
                paintDarkerColoredRect.setColor(Color.WHITE);
                break;
        }
    }
}
